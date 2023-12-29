using System;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class CommerceIntegrationRequestNormalizer : IPipelineBehavior<CommerceIntegrationRequest, Unit>
    {
        private readonly IOptionsSnapshot<CommerceIntegrationOptions> _optionsSnapshot;
        private readonly ILogger<CommerceIntegrationRequestNormalizer> _logger;

        private CommerceIntegrationOptions Options => _optionsSnapshot.Value;

        public CommerceIntegrationRequestNormalizer(
            IOptionsSnapshot<CommerceIntegrationOptions> optionsSnapshot,
            ILogger<CommerceIntegrationRequestNormalizer> logger)
        {
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
        }

        public Task<Unit> Handle(CommerceIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Sanitizing the commerce integration request {0} at index {1} of {2}.", request.Guid, request.Index, request.Total);

            request.Commerce.Ef = request.Commerce.Ef.PadLeft(6, '0');
            request.Commerce.Mcc.Code = request.Commerce.Mcc.Code.PadLeft(4, '0');

            foreach (var tpe in request.Commerce.Tpes)
            {
                tpe.NoSerie = tpe.NoSerie.TrimEnd('\t');
                tpe.NoSite = tpe.NoSite.PadLeft(3, '0');
            }

            foreach (var tpe in request.Commerce.Tpes)
            {
                foreach (var contrat in tpe.Contrats.Where(c => Options.SplitContracts.ContainsKey(c.Code)).ToList())
                {
                    foreach (var code in Options.SplitContracts[contrat.Code])
                    {
                        tpe.Contrats.Add(new Contrat
                        {
                            NoContrat = contrat.NoContrat,
                            Code = code,
                            DateDebut = contrat.DateDebut,
                            DateFin = contrat.DateFin
                        });
                    }
                }

                foreach (var contrat in tpe.Contrats.Where(c => !Options.IncludedContracts.Contains(c.Code)).ToList())
                {
                    tpe.Contrats.Remove(contrat);
                }
            }
            

            foreach (var tpe in request.Commerce.Tpes.Where(c => !Options.IncludedTpeStatus.Contains(c.Statut)).ToList())
            {
                request.Commerce.Tpes.Remove(tpe);
            }

            return next();
        }
    }
}
