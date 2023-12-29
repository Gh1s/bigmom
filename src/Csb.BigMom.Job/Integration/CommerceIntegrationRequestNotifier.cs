using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Job.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class CommerceIntegrationRequestNotifier : IPipelineBehavior<CommerceIntegrationRequest, Unit>
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, BalanceRequest> _balanceRequetProducerWrapper;
        private readonly KafkaProducerWrapper<string, DataRequest> _dataRequestProducerWrapper;
        private readonly KafkaProducerWrapper<string, CommerceIndexRequest> _commerceIndexRequestProducerWrapper;
        private readonly IOptionsSnapshot<CommerceIntegrationOptions> _commerceIntegrationOptions;
        private readonly IOptionsSnapshot<DataRequestOptions> _dataRequestOptionsSnapshot;
        private readonly IOptionsSnapshot<TraceOptions> _traceOptionsSnapshot;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<CommerceIntegrationRequestNotifier> _logger;

        private IProducer<string, BalanceRequest> BalanceRequestProducer => _balanceRequetProducerWrapper.Producer;

        private KafkaProducerOptions<BalanceRequest> BalanceRequestProducerOptions => _balanceRequetProducerWrapper.Options;

        private IProducer<string, DataRequest> DataRequestProducer => _dataRequestProducerWrapper.Producer;

        private KafkaProducerOptions<DataRequest> DataRequestProducerOptions => _dataRequestProducerWrapper.Options;

        private IProducer<string, CommerceIndexRequest> CommerceIndexRequestProducer => _commerceIndexRequestProducerWrapper.Producer;

        private KafkaProducerOptions<CommerceIndexRequest> CommerceIndexRequestOptions => _commerceIndexRequestProducerWrapper.Options;

        private CommerceIntegrationOptions CommerceIntegrationOptions => _commerceIntegrationOptions.Value;

        private DataRequestOptions DataRequestOptions => _dataRequestOptionsSnapshot.Value;

        private TraceOptions TraceOptions => _traceOptionsSnapshot.Value;

        public CommerceIntegrationRequestNotifier(
            BigMomContext context,
            KafkaProducerWrapper<string, BalanceRequest> balanceRequestProducerWrapper,
            KafkaProducerWrapper<string, DataRequest> dataRequestProducerWrapper,
            KafkaProducerWrapper<string, CommerceIndexRequest> commerceIndexRequestProducerWrapper,
            IOptionsSnapshot<CommerceIntegrationOptions> commerceIntegrationOptions,
            IOptionsSnapshot<DataRequestOptions> dataRequestOptionsSnapshot,
            IOptionsSnapshot<TraceOptions> traceOptionsSnapshot,
            ISystemClock systemClock,
            ILogger<CommerceIntegrationRequestNotifier> logger)
        {
            _context = context;
            _balanceRequetProducerWrapper = balanceRequestProducerWrapper;
            _dataRequestProducerWrapper = dataRequestProducerWrapper;
            _commerceIndexRequestProducerWrapper = commerceIndexRequestProducerWrapper;
            _commerceIntegrationOptions = commerceIntegrationOptions;
            _dataRequestOptionsSnapshot = dataRequestOptionsSnapshot;
            _traceOptionsSnapshot = traceOptionsSnapshot;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<Unit> Handle(CommerceIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            await Task.WhenAll(
                SendBalanceRequest(request),
                SendDataRequest(request),
                SendCommerceIndexRequest(request)
            );

            return Unit.Value;
        }

        private async Task SendBalanceRequest(CommerceIntegrationRequest request)
        {
            _logger.LogInformation("Verifying the commerce integration request {0} completion.", request.Guid);

            await _semaphore.WaitAsync();
            var handled = await _context.IntegrationTraces
                .OfType<CommerceIntegrationTrace>()
                .Where(p => p.Guid == request.Guid)
                .GroupBy(p => p.Index)
                .CountAsync(CancellationToken.None);
            _semaphore.Release();
            if (handled == request.Total -1)
            {
                _logger.LogInformation("The commerce integration request {0} is complete, triggering the telecollections balacing.", request.Guid);

                _logger.LogDebug("Sending the balance request.");
                var message = new Message<string, BalanceRequest>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        Date = _systemClock.UtcNow.LocalDateTime.Date,
                        Force = CommerceIntegrationOptions.ForceRebalance
                    }
                };
                await ProduceMessage(BalanceRequestProducer, BalanceRequestProducerOptions.Topic, message);
            }
            else
            {
                _logger.LogInformation("The commerce integration request {0} isn't complete, {1} out of {2} requests handled.", request.Guid, handled, request.Total);
            }
        }

        private async Task SendDataRequest(CommerceIntegrationRequest request)
        {
            _logger.LogInformation("Verifying if the commerce needs data from other systems.");

            if (request.Trace.Status != InfrastructureConstants.Integration.Status.Added &&
                request.Trace.Status != InfrastructureConstants.Integration.Status.Modified)
            {
                _logger.LogInformation("The commerce does not need data from other systems.");
                return;
            }

            var config = DataRequestOptions.Config[InfrastructureConstants.Data.Config.Idsa.Key];
            var dataTraces = new List<DataTrace>();
            var dataRequests = new List<DataRequest>();
            foreach (var tpe in request.Commerce.Tpes)
            {
                foreach (var app in tpe.Contrats.Where(a => CommerceIntegrationOptions.RequestIdsaForContracts.Contains(a.Code)))
                {
                    var traceIdentifier = Guid.NewGuid().ToString();
                    var dataRequest = new DataRequest
                    {
                        TraceIdentifier = traceIdentifier,
                        Job = config.Job,
                        Params = new()
                        {
                            { InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe, tpe.NoSerie },
                            { InfrastructureConstants.Data.Config.Idsa.Params.NoSiteTpe, tpe.NoSite },
                            { InfrastructureConstants.Data.Config.Idsa.Params.NoContrat, app.NoContrat },
                            { InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode, app.Code }
                        },
                        Data = config.Data
                    };
                    var dataTrace = new DataTrace
                    {
                        Id = traceIdentifier,
                        CreatedAt = _systemClock.UtcNow,
                        Payload = JsonSerializer.Serialize(dataRequest, TraceOptions.SerializationOptions)
                    };

                    dataRequests.Add(dataRequest);
                    dataTraces.Add(dataTrace);
                }
            }
            _logger.LogInformation("Storing {0} data traces.", dataTraces.Count);
            _context.DataTraces.AddRange(dataTraces);
            await _semaphore.WaitAsync();
            await _context.SaveChangesAsync();
            _semaphore.Release();

            _logger.LogInformation("Sending the requests.");
            await Task.WhenAll(dataRequests
                    .Select(m =>
                        ProduceMessage(
                            DataRequestProducer,
                            DataRequestProducerOptions.Topic,
                            new Message<string, DataRequest>
                            {
                                Key = Guid.NewGuid().ToString(),
                                Value = m
                            }
                        )
                    )
                    .ToArray()
            );
        }

        private async Task SendCommerceIndexRequest(CommerceIntegrationRequest request)
        {
            _logger.LogInformation("Verifying if the commerce has been updated or created in order to reindex.");

            if (request.Trace.Status != InfrastructureConstants.Integration.Status.Added &&
                request.Trace.Status != InfrastructureConstants.Integration.Status.Modified)
            {
                _logger.LogInformation("The commerce does not need to be reindexed.");
            }
            else
            {
                var message = new Message<string, CommerceIndexRequest>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = new()
                    {
                        CommerceId = request.Commerce.Id
                    }
                };
                _logger.LogDebug("Sending the reindex request for the commerce {0}.", request.Commerce.Id);
                await ProduceMessage(CommerceIndexRequestProducer, CommerceIndexRequestOptions.Topic, message);
            }
        }


        private async Task ProduceMessage<T>(IProducer<string, T> producer, string topic, Message<string, T> message)
        {
            _logger.LogTraceObject("{0} request message: ", message, typeof(T).FullName);

            var pr = await producer.ProduceAsync(topic, message, CancellationToken.None);
            if (pr.Status != PersistenceStatus.NotPersisted)
            {
                _logger.LogDebug("{0} request {1} delivered to topic {2} at partition {3} and offset {4}.",
                    typeof(T).FullName,
                    pr.Message.Key,
                    pr.Topic,
                    pr.TopicPartitionOffset.Partition,
                    pr.TopicPartitionOffset.Offset);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError("Failed to deliver the {0} request: {1}", typeof(T).FullName, JsonSerializer.Serialize(pr.Value));
                }
            }
        }

    }
}
