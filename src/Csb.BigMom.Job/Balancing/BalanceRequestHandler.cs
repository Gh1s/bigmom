using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Spreading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Balancing
{
    public class BalanceRequestHandler : IPipelineBehavior<BalanceRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly Balancer _balancer;
        private readonly IMediator _mediator;
        private readonly ILogger<BalanceRequestHandler> _logger;

        public BalanceRequestHandler(
            Balancer balancer,
            IMediator mediator,
            ILogger<BalanceRequestHandler> logger, BigMomContext context)
        {
            _balancer = balancer;
            _mediator = mediator;
            _logger = logger;
            _context = context;
        }

        public async Task<Unit> Handle(BalanceRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the balance request on {0} with forcing at {1}.", request.Date, request.Force);

            var apps = await _context.Applications
                .Include(a => a.Contrat)
                .ThenInclude(a => a.Tpe)
                .ThenInclude(a => a.Commerce)
                .ThenInclude(a => a.Mcc)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Balancing {0} applications.", apps.Count);
            var modifiedApps = _balancer.BalanceTelecollections(apps, request.Date, request.Force);
            _logger.LogInformation("Balancing done with {0} applications modified.", modifiedApps.Count());
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Sending the spread batch command.");
            await _mediator.Send(
                new SpreadBatchCommand
                {
                    Date = request.Date,
                    ModifiedApps = modifiedApps
                },
                cancellationToken
            );

            _logger.LogInformation("Balance request on {0} with forcing at {1} handled.", request.Date, request.Force);

            return Unit.Value;
        }
    }
}
