using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Data
{
    public class DataResponseHandler : IPipelineBehavior<DataResponse, Unit>
    {
        private readonly BigMomContext _context;
        private readonly ISystemClock _systemClock;
        private readonly IOptionsSnapshot<TraceOptions> _traceOptionsSnapshot;
        private readonly ILogger<DataResponseHandler> _logger;

        private TraceOptions TraceOptions => _traceOptionsSnapshot.Value;

        public DataResponseHandler(
            BigMomContext context,
            ISystemClock systemClock,
            IOptionsSnapshot<TraceOptions> traceOptionsSnapshot,
            ILogger<DataResponseHandler> logger)
        {
            _context = context;
            _systemClock = systemClock;
            _traceOptionsSnapshot = traceOptionsSnapshot;
            _logger = logger;
        }

        public async Task<Unit> Handle(DataResponse request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the data response {0}.", request.TraceIdentifier);

            var noSerieTpe = request.Params[InfrastructureConstants.Data.Config.Idsa.Params.NoSerieTpe];
            var noContrat = request.Params[InfrastructureConstants.Data.Config.Idsa.Params.NoContrat];
            var code = request.Params[InfrastructureConstants.Data.Config.Idsa.Params.ApplicationCode];

            var trace = await _context.DataTraces.SingleOrDefaultAsync(t => t.Id == request.TraceIdentifier, cancellationToken);
            if (trace == null)
            {
                _logger.LogWarning("There is no such trace: {0}", request.TraceIdentifier);
                return Unit.Value;
            }
            var traceResponse = new DataTraceResponse
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = _systemClock.UtcNow,
                Trace = trace,
                Payload = JsonSerializer.Serialize(request, TraceOptions.SerializationOptions)
            };

            var app = await _context.Applications
                .SingleOrDefaultAsync(
                    t =>
                        t.Contrat.Tpe.NoSerie == noSerieTpe &&
                        t.Contrat.NoContrat == noContrat &&
                        t.Contrat.Code == code,
                    cancellationToken
                );
            if (app != null)
            {
                try
                {
                    app.Idsa = request.Data["idsa"];
                    await _context.SaveChangesAsync(cancellationToken);
                    traceResponse.Status = InfrastructureConstants.Data.Statuses.Handled;
                    _logger.LogInformation("Data response {0} handled.", request.TraceIdentifier);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error has occured while updating the IDSA on TPE {0}.", noSerieTpe);
                    traceResponse.Status = InfrastructureConstants.Data.Statuses.Failed;
                    var entry = _context.Entry(app);
                    entry.State = EntityState.Unchanged;
                }
            }
            else
            {
                _logger.LogWarning("The application {0} doesn't exist on the TPE {1} with the contract number {2}.", code, noSerieTpe, noContrat);
                traceResponse.Status = InfrastructureConstants.Data.Statuses.NotFound;
            }

            _context.DataTraceResponses.Add(traceResponse);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}