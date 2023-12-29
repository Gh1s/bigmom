using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Spreading.Amx.Job.Spreading
{
    public class SpreadRequestHandler : IPipelineBehavior<SpreadRequest, Unit>
    {
        private readonly KafkaProducerWrapper<string, SpreadResponse> _producerWrapper;
        private readonly IOptionsSnapshot<AceOptions> _aceOptionsSnapshot;
        private readonly IOptionsSnapshot<JobOptions> _jobOptionsSnapshot;
        private readonly ILogger<SpreadRequestHandler> _logger;

        private IProducer<string, SpreadResponse> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<SpreadResponse> ProducerOptions => _producerWrapper.Options;

        private AceOptions AceOptions => _aceOptionsSnapshot.Value;

        private JobOptions JobOptions => _jobOptionsSnapshot.Value;

        public SpreadRequestHandler(
            KafkaProducerWrapper<string, SpreadResponse> producerWrapper,
            IOptionsSnapshot<AceOptions> aceOptionsSnapshot,
            IOptionsSnapshot<JobOptions> jobOptionsSnapshot,
            ILogger<SpreadRequestHandler> logger)
        {
            _producerWrapper = producerWrapper;
            _aceOptionsSnapshot = aceOptionsSnapshot;
            _jobOptionsSnapshot = jobOptionsSnapshot;
            _logger = logger;
        }

        public async Task<Unit> Handle(SpreadRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the spread request {0}.", request.TraceIdentifier);
            _logger.LogInformation("Controlling if this request has a site number between 01 and 49");

            if (int.Parse(request.NoSiteTpe) >= 1 && int.Parse(request.NoSiteTpe) <= 49)
            {
                _logger.LogInformation("This request has a site number between 01 and 49 we will send command to " +
                                       "this site number: {0} and site number + 50: {1} ", request.NoSiteTpe, int.Parse(request.NoSiteTpe + 50));
                SendCommand(request.NoContrat, request.HeureTlc.Value, request.NoSiteTpe, cancellationToken, request.TraceIdentifier);
                var newSiteNumber = "0" + (int.Parse(request.NoSiteTpe) + 50).ToString();
                SendCommand(request.NoContrat, request.HeureTlc.Value, newSiteNumber, cancellationToken, request.TraceIdentifier);
            }
            else
            {
                _logger.LogInformation("This request hasn't a site number between 01 and 49 we will send command to " +
                                       "this site number: {0} ", request.NoSiteTpe);
                SendCommand(request.NoContrat, request.HeureTlc.Value, request.NoSiteTpe, cancellationToken, request.TraceIdentifier);
            }

            return Unit.Value;
        }
        
        public async void SendCommand(string noContrat, DateTime heureTlc, string noSiteTpe, CancellationToken cancellationToken, 
            string traceIdentifier)
        {
            Message<string, SpreadResponse> message = null;
            
            try
            {
                var connection = new ConnectionInfo(
                        AceOptions.Host,
                        AceOptions.Port,
                        AceOptions.Username,
                        new PasswordAuthenticationMethod(
                            AceOptions.Username,
                            AceOptions.Password
                        )
                    )
                {
                    Timeout = TimeSpan.FromSeconds(AceOptions.TimeoutSeconds),
                    ChannelCloseTimeout = TimeSpan.FromSeconds(AceOptions.TimeoutSeconds)
                };
                using var client = new SshClient(connection);
                _logger.LogInformation("Establishing the connection the the remote server {0}.", AceOptions.Host);
                client.Connect();
                _logger.LogInformation("Connected to the remote server {0} with the user {1}, using password authentication.", AceOptions.Host, AceOptions.Username);

                _logger.LogDebug("Preparing the command.");
                var commandText = string.Format(
                    AceOptions.Command,
                    noContrat,
                    noSiteTpe.Substring(1, 2),
                    heureTlc.ToString("HHmm")
                );
                _logger.LogDebug("Command text: {0}", commandText);
                _logger.LogInformation("Sending the command.");

                using var command = client.CreateCommand(commandText, Encoding.UTF8);
                command.CommandTimeout = TimeSpan.FromSeconds(AceOptions.TimeoutSeconds);
                var attempt = 1;
                var success = false;
                while (attempt <= AceOptions.FailureThreshold && !success)
                {
                    try
                    {
                        _logger.LogDebug("Sending the command, attempt {0}/{1}.", attempt, AceOptions.FailureThreshold);
                        command.Execute();
                        success = true;
                    }
                    catch (SshOperationTimeoutException e)
                    {
                        _logger.LogError(e, "The SSH command has timedout at attempt {0}/{1}.", attempt, AceOptions.FailureThreshold);
                        _logger.LogDebug("Attempting to resend the command in {0} seconds.", AceOptions.FailureRetryDelaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(AceOptions.FailureRetryDelaySeconds), cancellationToken);
                        attempt++;
                    }
                }

                if (!success)
                {
                    _logger.LogWarning("Aborting the spread request handling as it reached the failture threshold after {0}.", attempt - 1);
                    message = new()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Value = new()
                        {
                            TraceIdentifier = traceIdentifier,
                            Job = JobOptions.JobName,
                            Status = InfrastructureConstants.Spreading.Statuses.Error,
                            Error = $"Aborted after {attempt - 1} timed out attempts."
                        }
                    };
                }
                else
                {
                    _logger.LogInformation("Command executed with the exit code {0}.", command.ExitStatus);
                    _logger.LogDebug("Command result:{0}{1}", Environment.NewLine, command.Result);

                    if (command.ExitStatus == 0)
                    {
                        message = new()
                        {
                            Key = Guid.NewGuid().ToString(),
                            Value = new()
                            {
                                TraceIdentifier = traceIdentifier,
                                Job = JobOptions.JobName,
                                Status = InfrastructureConstants.Spreading.Statuses.Updated
                            }
                        };
                        _logger.LogInformation("Spread request {0} handled.", traceIdentifier);
                    }
                    else
                    {
                        message = new()
                        {
                            Key = Guid.NewGuid().ToString(),
                            Value = new()
                            {
                                TraceIdentifier = traceIdentifier,
                                Job = JobOptions.JobName,
                                Status = InfrastructureConstants.Spreading.Statuses.Error,
                                Error = $"The command returned an exit code {command.ExitStatus}."
                            }
                        };
                        _logger.LogWarning("Failed to handle the spread request {0}.", traceIdentifier);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while handling the spread request {0}.", traceIdentifier);
                message = new()
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        TraceIdentifier = traceIdentifier,
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
        }
    }
}
