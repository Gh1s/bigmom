using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job.Integration
{
    public class CommerceIntegrationRequestHandler : IPipelineBehavior<CommerceIntegrationRequest, Unit>
    {
        private readonly CommerceHashComputer _hashComputer;
        private readonly BigMomContext _context;
        private readonly ILogger<CommerceIntegrationRequestHandler> _logger;

        public CommerceIntegrationRequestHandler(
            CommerceHashComputer hashComputer,
            BigMomContext context,
            ILogger<CommerceIntegrationRequestHandler> logger)
        {
            _hashComputer = hashComputer;
            _context = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(CommerceIntegrationRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
        {
            _logger.LogInformation("Handling the commerce integration request {0} at index {1} of {2}.", request.Guid, request.Index, request.Total);
            _logger.LogTraceObject("Integration trace: ", request.Trace);

            try
            {
                var commerce = await _context.Commerces
                    .Include(c => c.Mcc)
                    .Include(c => c.Tpes)
                    .ThenInclude(c => c.Contrats)
                    .SingleOrDefaultAsync(c => c.Identifiant == request.Commerce.Identifiant, cancellationToken);
                if (commerce != null)
                {
                    _logger.LogInformation("Commerce {0} found.", request.Commerce.Identifiant);

                    var hash = _hashComputer.ComputeHash(request.Commerce);
                    if (hash != commerce.Hash)
                    {
                        _logger.LogDebug("The computed hash is different than the stored one for the commerce {0}, merging data.", request.Commerce.Identifiant);
                        await Merge(request.Commerce, commerce);
                        commerce.Hash = hash;
                        request.Trace.Status = InfrastructureConstants.Integration.Status.Modified;
                    }
                    else
                    {
                        _logger.LogDebug("The computed hash is the same as the stored one for the commerce {0}.", request.Commerce.Identifiant);
                        request.Trace.Status = InfrastructureConstants.Integration.Status.Unchanged;
                    }
                }
                else
                {
                    _logger.LogInformation("The commerce doesn't exists, creating it.");

                    commerce = request.Commerce;
                    await Init(commerce);
                    commerce.Hash = _hashComputer.ComputeHash(commerce);
                    _context.Commerces.Add(commerce);
                    request.Trace.Status = InfrastructureConstants.Integration.Status.Added;
                }

                _logger.LogInformation("Saving changes.");
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Commerce integration request {0} at index {1} of {2} handled.", request.Guid, request.Index, request.Total);
            }
            catch (Exception e)
            {
                request.Trace.Status = InfrastructureConstants.Integration.Status.Failed;
                request.Trace.Error = e.ToString();

                _logger.LogError(e, "Commerce integration request {0} at index {1} of {2} failed.", request.Guid, request.Index, request.Total);
            }
            finally
            {
                _logger.LogInformation("Saving trace.");

                _context.ChangeTracker.Clear();
                _context.IntegrationTraces.Add(request.Trace);
                await _context.SaveChangesAsync(CancellationToken.None);
            }

            return await next();
        }

        private async Task Init(Commerce commerce)
        {
            commerce.Mcc = await _context.Mccs.SingleOrDefaultAsync(c => c.Code == commerce.Mcc.Code);
            foreach (var tpe in commerce.Tpes)
            {
                foreach (var contrat in tpe.Contrats)
                {
                    contrat.Applications = tpe.Contrats.Select(c => new Application
                    {
                        Contrat = c
                    }).Where(c => contrat.Code.Equals(c.Contrat.Code) && contrat.NoContrat.Equals(c.Contrat.NoContrat)).ToList();
                }
            }
        }

        private async Task Merge(Commerce source, Commerce destination)
        {
            // Merging commerce info.
            destination.Mcc = await _context.Mccs.SingleOrDefaultAsync(c => c.Code == source.Mcc.Code);
            destination.Nom = source.Nom;
            destination.Email = source.Email;
            destination.Ef = source.Ef;
            destination.DateAffiliation = source.DateAffiliation;
            destination.DateResiliation = source.DateResiliation;

            var tpeComparer = new TpeEqualityComparer();
            // Removing obsolete TPE.
            foreach (var tpe in destination.Tpes.Except(source.Tpes, tpeComparer))
            {
                destination.Tpes.Remove(tpe);
            }
            // Merging existing TPEs.
            foreach (var pair in destination.Tpes.Join(source.Tpes, a => a.NoSerie, b => b.NoSerie, (d, s) => new { d, s }))
            {
                pair.d.NoSite = pair.s.NoSite;
                pair.d.Modele = pair.s.Modele;
                pair.d.Statut = pair.s.Statut;
            }
            // Adding new TPEs.
            foreach (var tpe in source.Tpes.Except(destination.Tpes, tpeComparer))
            {
                tpe.Commerce = destination;
                destination.Tpes.Add(tpe);
            }

            foreach (var tpeDest in destination.Tpes)
            {
                foreach (var tpeSour in source.Tpes)
                {
                    var contratComparer = new ContratEqualityComparer();
                    // Removing obsolete contrats.
                    foreach (var contrat in tpeDest.Contrats.Except(tpeSour.Contrats, contratComparer))
                    {
                        tpeDest.Contrats.Remove(contrat);
                        foreach (var app in contrat.Applications.Where(a => a.Contrat.NoContrat == contrat.NoContrat))
                        {
                            contrat.Applications.Remove(app);
                        }
                    }

                    // Merging existing contrats.
                    foreach (var pair in tpeDest.Contrats.Join(tpeSour.Contrats, a => a, b => b, (d, s) => new {d, s},
                                 contratComparer))
                    {
                        pair.d.Code = pair.s.Code;
                        pair.d.DateDebut = pair.s.DateDebut;
                        pair.d.DateFin = pair.s.DateFin;
                    }

                    // Adding new contrats.
                    foreach (var contrat in tpeSour.Contrats.Except(tpeDest.Contrats, contratComparer))
                    {
                        //contrat.Commerce = destination;
                        contrat.Tpe.Commerce = destination;
                        tpeDest.Contrats.Add(contrat);
                    }

                    foreach (var contrat in tpeDest.Contrats)
                    {
                        var app = contrat.Applications.First(a =>
                            a.Contrat.NoContrat == contrat.NoContrat && a.Contrat.Code == contrat.Code &&
                            a.Contrat.DateDebut == contrat.DateDebut);
                        if (app != null)
                        {
                            app = new()
                            {
                                //Tpe = tpe,
                                Contrat = contrat
                            };
                            contrat.Applications.Add(app);
                        }
                    }
                    
                }
            }
            
        }

        private class ContratEqualityComparer : IEqualityComparer<Contrat>
        {
            public bool Equals(Contrat x, Contrat y) => x.NoContrat == y.NoContrat && x.Code == y.Code && x.DateDebut == y.DateDebut;

            public int GetHashCode([DisallowNull] Contrat obj) => HashCode.Combine(obj.NoContrat, obj.Code, obj.DateDebut);
        }

        private class TpeEqualityComparer : IEqualityComparer<Tpe>
        {
            public bool Equals(Tpe x, Tpe y) => x.NoSerie == y.NoSerie;

            public int GetHashCode([DisallowNull] Tpe obj) => obj.NoSerie.GetHashCode();
        }
    }
}
