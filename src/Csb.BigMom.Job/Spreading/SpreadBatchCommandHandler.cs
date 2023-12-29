using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Job.Balancing;
using MediatR;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Spreading
{
    public class SpreadBatchCommandHandler : IRequestHandler<SpreadBatchCommand>
    {
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, SpreadRequest> _producerWrapper;
        private readonly IOptionsSnapshot<SpreadOptions> _spreadOptionsSnapshot;
        private readonly IOptionsSnapshot<TraceOptions> _traceOptionsSnapshot;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<BalanceRequestHandler> _logger;

        private IProducer<string, SpreadRequest> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<SpreadRequest> ProducerOptions => _producerWrapper.Options;

        private SpreadOptions SpreadOptions => _spreadOptionsSnapshot.Value;

        private TraceOptions TraceOptions => _traceOptionsSnapshot.Value;

        public SpreadBatchCommandHandler(
            BigMomContext context,
            KafkaProducerWrapper<string, SpreadRequest> producerWrapper,
            IOptionsSnapshot<SpreadOptions> spreadOptionsSnapshot,
            IOptionsSnapshot<TraceOptions> traceOptionsSnapshot,
            ISystemClock systemClock,
            ILogger<BalanceRequestHandler> logger)
        {
            _context = context;
            _producerWrapper = producerWrapper;
            _spreadOptionsSnapshot = spreadOptionsSnapshot;
            _traceOptionsSnapshot = traceOptionsSnapshot;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<Unit> Handle(SpreadBatchCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling the spread batch command at {1}.", request.Date);

            var modifiedApps = request.ModifiedApps.ToList();
            if (SpreadOptions.IncludedCommerces.Any())
            {
                _logger.LogDebug("Filtering the spread output.");
                var removed = modifiedApps.RemoveAll(p => !SpreadOptions.IncludedCommerces.Contains(p.Contrat.Tpe.Commerce.Identifiant));
                _logger.LogDebug("{0} commerce(s) removed.", removed);
            }

            var spreadTraces = new List<SpreadTrace>();
            var spreadRequests = new List<SpreadRequest>();
            foreach (var app in modifiedApps)
            {
                var traceIdentifier = Guid.NewGuid().ToString();
                var spreadRequest = new SpreadRequest
                {
                    TraceIdentifier = traceIdentifier,
                    CommerceId = app.Contrat.Tpe.Commerce.Identifiant,
                    NoSerieTpe = app.Contrat.Tpe.NoSerie,
                    NoSiteTpe = app.Contrat.Tpe.NoSite.PadLeft(3, '0'),
                    NoContrat = app.Contrat.NoContrat,
                    ApplicationCode = app.Contrat.Code,
                    HeureTlc = app.HeureTlc.HasValue ? request.Date.Date.Add(app.HeureTlc.Value) : null,
                    Ef = app.Contrat.Tpe.Commerce.Ef.PadLeft(6, '0')
                };
                var spreadTrace = new SpreadTrace
                {
                    Id = traceIdentifier,
                    CreatedAt = _systemClock.UtcNow,
                    Application = app,
                    HeureTlc = app.HeureTlc,
                    Payload = JsonSerializer.Serialize(spreadRequest, TraceOptions.SerializationOptions)
                };

                spreadTraces.Add(spreadTrace);
                spreadRequests.Add(spreadRequest);
            }
            _logger.LogInformation("Storing {0} spread traces.", spreadTraces.Count);
            _context.SpreadTraces.AddRange(spreadTraces);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sending the spread requests.");
            Parallel.ForEach(
                spreadRequests,
                request =>
                {
                    _logger.LogDebug("Sending the spread request for terminal {0} and application {1}.", request.NoSerieTpe, request.ApplicationCode);

                    var message = new Message<string, SpreadRequest>
                    {
                        Key = Guid.NewGuid().ToString(),
                        Value = request,
                        Timestamp = Timestamp.Default
                    };
                    _logger.LogTraceObject("Spread request message: ", message);

                    Producer.Produce(
                        ProducerOptions.Topic,
                        message,
                        pr =>
                        {
                            if (pr.Status != PersistenceStatus.NotPersisted)
                            {
                                _logger.LogDebug(
                                    "Spread request delivered to topic {0} at partition {1} and offset {2}.",
                                    pr.Topic,
                                    pr.TopicPartitionOffset.Partition,
                                    pr.TopicPartitionOffset.Offset
                                );
                            }
                            else
                            {
                                if (_logger.IsEnabled(LogLevel.Error))
                                {
                                    _logger.LogError("Failed to deliver the spread request: {0}", JsonSerializer.Serialize(pr.Value));
                                }
                            }
                        }
                    );
                }
            );
            Producer.Flush(CancellationToken.None);

            return Unit.Value;
        }
    }
}
