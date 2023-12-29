using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Data
{
    public class TlcIndexRequestHandler : IPipelineBehavior<TlcIndexRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<TlcIndexRequestHandler> _logger;

        public TlcIndexRequestHandler(
            BigMomContext context,
            IElasticClient elasticClient,
            ILogger<TlcIndexRequestHandler> logger)
        {
            _context = context;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(TlcIndexRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the index request for the TLC {0}.", request.TlcId);

            var tlc = await _context.Tlcs
                .Include(t => t.App)
                .Include(t => t.App.Contrat)
                .Include(t => t.App.Contrat.Tpe)
                .Include(t => t.App.Contrat.Tpe.Commerce)
                .Include(t => t.App.Contrat.Tpe.Commerce.Mcc)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == request.TlcId, cancellationToken);
            if (tlc != null)
            {
                _logger.LogDebug("Indexing the TLC.");

                var result = await _elasticClient.IndexDocumentAsync(tlc, cancellationToken);
                if (result.IsValid)
                {
                    _logger.LogInformation("Index request handled successfully for the TLC {0}.", request.TlcId);
                }
                else
                {
                    _logger.LogError(result.OriginalException, "An error has occured while indexing the TLC {0}.", request.TlcId);
                    _logger.LogDebug(result.DebugInformation);
                }
            }
            else
            {
                _logger.LogWarning("The TLC {0} does not exist.", request.TlcId);
            }

            return Unit.Value;
        }
    }
}
