using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class CommerceIntegrationRequestValidator : IPipelineBehavior<CommerceIntegrationRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsSnapshot<CommerceIntegrationOptions> _integrationOptionsSnapshot;
        private readonly IOptionsSnapshot<TraceOptions> _traceOptionsSnapshot;
        private readonly ILogger<CommerceIntegrationRequestValidator> _logger;

        private CommerceIntegrationOptions IntegrationOptions => _integrationOptionsSnapshot.Value;

        private TraceOptions TraceOptions => _traceOptionsSnapshot.Value;

        public CommerceIntegrationRequestValidator(
            BigMomContext context,
            ISystemClock systemClock,
            IOptionsSnapshot<CommerceIntegrationOptions> integrationOptionsSnapshot,
            IOptionsSnapshot<TraceOptions> traceOptionsSnapshot,
            ILogger<CommerceIntegrationRequestValidator> logger)
        {
            _context = context;
            _systemClock = systemClock;
            _integrationOptionsSnapshot = integrationOptionsSnapshot;
            _traceOptionsSnapshot = traceOptionsSnapshot;
            _logger = logger;
        }

        public async Task<Unit> Handle(CommerceIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Validating the commerce integration request {0} at index {1} of {2}.", request.Guid, request.Index, request.Total);

            if (request.Guid is null || request.Total == default || request.Commerce is null)
            {
                _logger.LogWarning("Invalid integration request.");
                return Unit.Value;
            }

            request.Trace = new()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = _systemClock.UtcNow,
                Guid = request.Guid,
                Index = request.Index,
                Total = request.Total,
                Payload = JsonSerializer.Serialize(request.Commerce, TraceOptions.SerializationOptions)
            };

            if (IntegrationOptions.ExcludedCommerces.Contains(request.Commerce.Identifiant))
            {
                _logger.LogWarning("Integration request excluded.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.Excluded;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            if (!ValidatePayload(request.Commerce))
            {
                _logger.LogWarning("Invalid integration request payload.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidPayload;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            if (!await _context.Mccs.AnyAsync(m => m.Code == request.Commerce.Mcc.Code.PadLeft(4, '0')))
            {
                _logger.LogWarning("Invalid integration request MCC.");
                request.Trace.Status = InfrastructureConstants.Integration.Status.InvalidMcc;
                _context.Add(request.Trace);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }

            return await next();
        }

        private static bool ValidatePayload(Commerce commerce) =>
            commerce.Identifiant != null &&
            commerce.Mcc?.Code != null &&
            commerce.Nom != null &&
            commerce.Ef != null &&
            /*commerce.Tpes.All(c =>
                /*c.Contrat.NoContrat != null &&
                c.Contrat.Code != null#1#
                c.Contrats.All(c =>
                    c.NoContrat != null &&
                    c.Code != null)
            ) &&*/
            commerce.Tpes.All(t =>
                t.NoSerie != null &&
                t.NoSite != null &&
                t.Modele != null &&
                t.Statut != null
            );
    }
}
