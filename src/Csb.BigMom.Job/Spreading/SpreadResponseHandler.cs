using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Spreading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Spreading
{
    public class SpreadResponseHandler : IPipelineBehavior<SpreadResponse, Unit>
    {
        private readonly BigMomContext _context;
        private readonly ISystemClock _systemClock;
        private readonly ILogger<SpreadResponseHandler> _logger;

        public SpreadResponseHandler(BigMomContext context, ISystemClock systemClock, ILogger<SpreadResponseHandler> logger)
        {
            _context = context;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<Unit> Handle(SpreadResponse request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the spread response of {0} to the request{1}.", request.Job, request.TraceIdentifier);

            var trace = await _context.SpreadTraces.SingleOrDefaultAsync(t => t.Id == request.TraceIdentifier, cancellationToken);
            if (trace is null)
            {
                _logger.LogError("There is no such spread trace {0}.", request.TraceIdentifier);
                return Unit.Value;
            }

            _logger.LogInformation("Storing the spread response of the request {0}.", request.TraceIdentifier);
            try
            {
                var response = new SpreadTraceResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = _systemClock.UtcNow,
                    Job = request.Job,
                    Status = request.Status,
                    Trace = trace,
                    Error = request.Error
                };
                _context.SpreadTraceResponses.Add(response);
                _logger.LogInformation("Saving changes.");
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occured while storing the spread response of the request {0}.", request.TraceIdentifier);
            }

            _logger.LogInformation("Spread response of {0} to the request{1} handled.", request.Job, request.TraceIdentifier);

            return Unit.Value;
        }
    }
}
