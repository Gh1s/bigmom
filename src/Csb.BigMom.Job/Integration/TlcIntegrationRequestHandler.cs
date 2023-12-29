using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class TlcIntegrationHandler : IPipelineBehavior<TlcIntegrationRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly KafkaProducerWrapper<string, TlcIndexRequest> _producerWrapper;
        private readonly ILogger<TlcIntegrationHandler> _logger;

        private IProducer<string, TlcIndexRequest> Producer => _producerWrapper.Producer;

        private KafkaProducerOptions<TlcIndexRequest> ProducerOptions => _producerWrapper.Options;

        public TlcIntegrationHandler(
            BigMomContext context,
            KafkaProducerWrapper<string, TlcIndexRequest> producerWrapper,
            ILogger<TlcIntegrationHandler> logger)
        {
            _context = context;
            _producerWrapper = producerWrapper;
            _logger = logger;
        }

        public async Task<Unit> Handle(TlcIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the telecollection request from Powercard Table {0} ", request.Table);
            _logger.LogTraceObject("Integration trace: ", request.Trace);

            try
            {
                if (request.OpType == "I")
                {
                    var app = await _context.Applications.SingleOrDefaultAsync(
                        t => t.Idsa == request.After.Idsa,
                        cancellationToken
                    );
                    if (app != null)
                    {
                        var tlc = new Infrastructure.Entities.Tlc();
                        InitTlc(tlc, request);
                        app.Tlcs.Add(tlc);
                        await _context.SaveChangesAsync(cancellationToken);
                        await IndexTlc(tlc);
                        request.Trace.Status = InfrastructureConstants.Integration.Status.Added;
                    }
                    else
                    {
                        _logger.LogWarning("IDSA is null, this transaction doesn't have a valid terminal POS number.");
                        request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidIdsa;
                    }
                }
                else if (request.OpType == "U")
                {
                    var tlc = await _context.Tlcs.SingleOrDefaultAsync(
                        t => t.App.Idsa == request.After.Idsa && t.ProcessingDate == DateTimeOffset.Parse(request.After.ProcessingDate),
                        cancellationToken
                    );
                    if (tlc != null)
                    {
                        InitTlc(tlc, request);
                        await _context.SaveChangesAsync(cancellationToken);
                        await IndexTlc(tlc);
                        request.Trace.Status = InfrastructureConstants.Integration.Status.Modified;
                    }

                    _logger.LogWarning("This telecollection doesn't exist in database");
                    request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidTelecollection;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "A error has occured while trying to store the telecollection.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.Failed;
                request.Trace.Error = e.ToString();
            }
            finally
            {
                _logger.LogInformation("Saving trace.");

                _context.ChangeTracker.Clear();
                _context.IntegrationTraces.Add(request.Trace);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            static void InitTlc(Infrastructure.Entities.Tlc tlc, TlcIntegrationRequest request)
            {
                tlc.ProcessingDate = DateTimeOffset.Parse(request.After.ProcessingDate);
                tlc.Status = request.After.TransactionDowloadStatus == "ES" ? "OK" : "KO";
                tlc.NbTransactionsDebit = request.After.NbrDebit;
                tlc.NbTransactionsCredit = request.After.NbrCredit;
                tlc.TotalDebit = request.After.TotalDebit;
                tlc.TotalCredit = request.After.TotalCredit;
                tlc.TotalReconcilie = request.After.TotalReconcilie;
            }

            async Task IndexTlc(Infrastructure.Entities.Tlc tlc)
            {
                var pr = await Producer.ProduceAsync(
                    ProducerOptions.Topic,
                    new()
                    {
                        Key = Guid.NewGuid().ToString(),
                        Value = new()
                        {
                            TlcId = tlc.Id
                        }
                    }
                );
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
                    if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Error))
                    {
                        _logger.LogError("Failed to deliver the {0} request: {1}", typeof(TlcIndexRequest).FullName, JsonSerializer.Serialize(pr.Value));
                    }
                }
            }

            return await next();
        }
    }
}