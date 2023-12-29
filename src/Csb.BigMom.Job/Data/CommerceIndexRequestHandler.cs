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
    public class CommerceIndexRequestHandler : IPipelineBehavior<CommerceIndexRequest, Unit>
    {
        private readonly BigMomContext _context;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<CommerceIndexRequestHandler> _logger;

        public CommerceIndexRequestHandler(
            BigMomContext context,
            IElasticClient elasticClient,
            ILogger<CommerceIndexRequestHandler> logger)
        {
            _context = context;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(CommerceIndexRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the index request for the commerce {0}.", request.CommerceId);

            var commerce = await _context.Commerces
                .Include(c => c.Tpes).ThenInclude(t => t.Contrats)
                .Include(c => c.Tpes).ThenInclude(t => t.Contrats).ThenInclude(t => t.Applications).ThenInclude(t => t.Tlcs)
                .Include(c => c.Tpes)
                .Include(c => c.Mcc)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == request.CommerceId, cancellationToken);
            if (commerce != null)
            {
                _logger.LogDebug("Indexing the commerce.");

                var result = await _elasticClient.IndexDocumentAsync(commerce, cancellationToken);
                if (result.IsValid)
                {
                    _logger.LogInformation("Index request handled successfully for the commerce {0}.", request.CommerceId);
                }
                else
                {
                    _logger.LogError(result.OriginalException, "An error has occured while indexing the commerce {0}.", request.CommerceId);
                    _logger.LogDebug(result.DebugInformation);
                }
            }
            else
            {
                _logger.LogWarning("The commerce {0} does not exist.", request.CommerceId);
            }

            return Unit.Value;
        }
    }
}