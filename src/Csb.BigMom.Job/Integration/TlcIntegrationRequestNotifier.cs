using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class TlcIntegrationRequestNotifier : IPipelineBehavior<TlcIntegrationRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, CommerceIndexRequest> _producerWrapper;
        private readonly ILogger<TlcIntegrationRequestNotifier> _logger;

        public TlcIntegrationRequestNotifier(
            BigMomContext context,
            KafkaProducerWrapper<string, CommerceIndexRequest> producerWrapper,
            ILogger<TlcIntegrationRequestNotifier> logger)
        {
            _context = context;
            _producerWrapper = producerWrapper;
            _logger = logger;
        }

        public async Task<Unit> Handle(TlcIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Notifying the telecollection integration request completion.");

            if (request.Trace.Status == InfrastructureConstants.Integration.Status.Added || 
                request.Trace.Status == InfrastructureConstants.Integration.Status.Modified)
            {
                _logger.LogInformation("The telecollection integration has modified the commerce, triggering a reindex.");
                _logger.LogDebug("Fetching the commerce id from the database.");
                var message = new Message<string, CommerceIndexRequest>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        CommerceId = await _context.Applications
                            .Where(c => c.Idsa == request.After.Idsa)
                            .Select(c => c.Contrat.Tpe.Commerce.Id)
                            .SingleOrDefaultAsync(cancellationToken)
                    }
                };
                _logger.LogDebug("Giving the order to reindex into elasticsearch for the following commerce {0}.", message.Value.CommerceId);
                var topic = _producerWrapper.Options.Topic;
                var pr = await _producerWrapper.Producer.ProduceAsync(topic, message, CancellationToken.None);
                if (pr.Status != PersistenceStatus.NotPersisted)
                {
                    _logger.LogDebug("{0} request {1} delivered to topic {2} at partition {3} and offset {4}.",
                        typeof(CommerceIndexRequest).FullName,
                        pr.Message.Key,
                        pr.Topic,
                        pr.TopicPartitionOffset.Partition,
                        pr.TopicPartitionOffset.Offset);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError("Failed to deliver the {0} request: {1}", typeof(CommerceIndexRequest).FullName, JsonSerializer.Serialize(pr.Value));
                    }
                }

            }
            else
            {
                _logger.LogWarning("The commerce associated to the IDSA {0} did not change.", request.After.Idsa);
            }

            return Unit.Value;
        }
    }
}