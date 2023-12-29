using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using MediatR;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Spreading.Jcb.Job.Spreading
{
    public class SpreadRequestHandler : IPipelineBehavior<SpreadRequest, Unit>
    {
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly IOptionsSnapshot<JobOptions> _jobOptionsSnapshot;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<SpreadRequestHandler> _logger;

        private IProducer<string, SpreadResponse> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<SpreadResponse> ProducerOptions => _producerWrapper.Options;

        private JobOptions JobOptions => _jobOptionsSnapshot.Value;

        public SpreadRequestHandler(
            KafkaProducerWrapper<string, SpreadResponse> producerWrapper,
            IOptionsSnapshot<JobOptions> jobOptionsSnapshot,
            ISystemClock systemClock,
            ILogger<SpreadRequestHandler> logger)
        {
            _producerWrapper = producerWrapper;
            _jobOptionsSnapshot = jobOptionsSnapshot;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<Unit> Handle(SpreadRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the spread request {0}.", request.TraceIdentifier);

            Message<string, SpreadResponse> message = null;

            try
            {
                var fileName = string.Format(JobOptions.OutFilePathTemplate, _systemClock.UtcNow.LocalDateTime);
                var tryOpenFile = true;
                var tryAppenFile = true;

                while (tryOpenFile)
                {
                    try
                    {
                        using var file = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                        using var reader = new StreamReader(file, Encoding.UTF8);
                        tryOpenFile = false;

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            var fields = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                            if (fields[0] == request.NoContrat &&
                                fields[1] == request.NoSiteTpe &&
                                fields[2] == request.HeureTlc.Value.ToString("HHmm"))
                            {
                                _logger.LogWarning(
                                    "La ligne [{0},{1},{2}] existe déjà dans le fichier {3}.",
                                    fields[0],
                                    fields[1],
                                    fields[2],
                                    fileName
                                );
                                tryAppenFile = false;
                            }
                        }

                        if (tryAppenFile)
                        {
                            file.Seek(0, SeekOrigin.End);
                            using var writer = new StreamWriter(file, Encoding.UTF8);
                            await writer.WriteLineAsync($"{request.NoContrat};{request.NoSiteTpe};{request.HeureTlc.Value:HHmm}");
                            await writer.FlushAsync();
                        }
                    }
                    catch (IOException e)
                    {
                        _logger.LogWarning(e, "The file {0} is in use.", fileName);
                        await Task.Delay(JobOptions.OutFileLockCheckIntervalMs);
                    }
                }

                message = new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = request.TraceIdentifier,
                        Job = JobOptions.JobName,
                        Status = InfrastructureConstants.Spreading.Statuses.Updated
                    }
                };
                _logger.LogInformation("Spread request {0} handled.", request.TraceIdentifier);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the spread request {0}.", request.TraceIdentifier);
                message = new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = request.TraceIdentifier,
                        Job = JobOptions.JobName,
                        Status = InfrastructureConstants.Spreading.Statuses.Error,
                        Error = e.ToString()
                    }
                };
            }
            finally
            {
                await Producer.ProduceAsync(ProducerOptions.Topic, message);
            }

            return Unit.Value;
        }
    }
}
