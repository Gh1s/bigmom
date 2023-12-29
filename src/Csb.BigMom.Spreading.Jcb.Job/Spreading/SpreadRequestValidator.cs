using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Spreading.Jcb.Job.Spreading
{
    public class SpreadRequestValidator : IPipelineBehavior<SpreadRequest, Unit>
    {
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly IOptionsSnapshot<JobOptions> _jobOptionsSnapshot;
        private readonly ILogger<SpreadRequestValidator> _logger;

        private IProducer<string, SpreadResponse> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<SpreadResponse> ProducerOptions => _producerWrapper.Options;

        private JobOptions JobOptions => _jobOptionsSnapshot.Value;

        public SpreadRequestValidator(
            KafkaProducerWrapper<string, SpreadResponse> producerWrapper,
            IOptionsSnapshot<JobOptions> jobOptionsSnapshot,
            ILogger<SpreadRequestValidator> logger)
        {
            _producerWrapper = producerWrapper;
            _jobOptionsSnapshot = jobOptionsSnapshot;
            _logger = logger;
        }

        public Task<Unit> Handle(SpreadRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Validating the spread request {0}.", request.TraceIdentifier);

            if (request.TraceIdentifier == null)
            {
                _logger.LogWarning("The spread request misses a trace identifier.");
                return Task.FromResult(Unit.Value);
            }

            Message<string, SpreadResponse> message = null;

            if (string.IsNullOrWhiteSpace(request.NoContrat) ||
                string.IsNullOrWhiteSpace(request.NoSiteTpe) ||
                request.HeureTlc == null)
            {
                _logger.LogWarning("Invalid spread request {0}.", request.TraceIdentifier);
                message = new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = request.TraceIdentifier,
                        Job = JobOptions.JobName,
                        Status = InfrastructureConstants.Spreading.Statuses.Error,
                        Error = "Invalid payload"
                    }
                };
            }

            if (!JobOptions.SupportedApps.Contains(request.ApplicationCode))
            {
                _logger.LogInformation("Unsupported application {0}.", request.ApplicationCode);
                message = new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = request.TraceIdentifier,
                        Job = JobOptions.JobName,
                        Status = InfrastructureConstants.Spreading.Statuses.Ignored
                    }
                };
            }

            if (message != null)
            {
                return Producer.ProduceAsync(ProducerOptions.Topic, message, cancellationToken).ContinueWith(_ => Unit.Value);
            }

            return next();
        }
    }
}
