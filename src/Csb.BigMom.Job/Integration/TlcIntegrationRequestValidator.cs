using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class TlcIntegrationValidator : IPipelineBehavior<TlcIntegrationRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsSnapshot<TlcIntegrationOptions> _tlcOptionsSnapshot;
        private readonly IOptionsSnapshot<TraceOptions> _traceOptionsSnapshot;
        private readonly ILogger<TlcIntegrationValidator> _logger;

        private TlcIntegrationOptions TlcOptions => _tlcOptionsSnapshot.Value;

        private TraceOptions TraceOptions => _traceOptionsSnapshot.Value;

        public TlcIntegrationValidator(
            BigMomContext context,
            ISystemClock systemClock,
            IOptionsSnapshot<TlcIntegrationOptions> tlcOptionsSnapshot,
            IOptionsSnapshot<TraceOptions> traceOptionsSnapshot,
            ILogger<TlcIntegrationValidator> logger)
        {
            _context = context;
            _systemClock = systemClock;
            _tlcOptionsSnapshot = tlcOptionsSnapshot;
            _traceOptionsSnapshot = traceOptionsSnapshot;
            _logger = logger;
        }

        public async Task<Unit> Handle(TlcIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Validating the TLC integration request.");

            request.Trace = new()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = _systemClock.UtcNow,
                Table = request.Table,
                OpType = request.OpType,
                Timestamp = DateTimeOffset.Parse(request.Timestamp),
                Payload = JsonSerializer.Serialize(request.After, TraceOptions.SerializationOptions)
            };

            if (!TlcOptions.TableNames.Contains(request.Table))
            {
                _logger.LogWarning("Unsupported table {0}", request.Table);
                request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidTable;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            if (!ValidatePayload(request))
            {
                _logger.LogWarning("Invalid integration request payload.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidPayload;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            _logger.LogInformation("Controlling if the message is a telecollection.");

            if (request.After.ConnectionType != "DT")
            {
                _logger.LogWarning("This request isn't a telecollection putting this message aside.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidConnectionType;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            return await next();
        }

        private static bool ValidatePayload(TlcIntegrationRequest request) =>
            request.OpType != null &&
            request.After != null &&
            request.After.Idsa != null &&
            request.After.TransactionDowloadStatus != null &&
            request.After.ConnectionType != null;
    }
}