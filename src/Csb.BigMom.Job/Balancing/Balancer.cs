using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csb.BigMom.Job.Balancing
{
    /// <summary>
    /// Provides an utility to balance telecollections over the ranges.
    /// </summary>
    public class Balancer
    {
        private readonly IOptionsSnapshot<BalancerOptions> _optionsSnapshot;

        private BalancerOptions Options => _optionsSnapshot.Value;

        public Balancer(IOptionsSnapshot<BalancerOptions> optionsSnapshot)
        {
            _optionsSnapshot = optionsSnapshot;
        }

        /// <summary>
        /// Balances telecollections of the provided apps and return the modified one.
        /// </summary>
        /// <param name="apps">The apps.</param>
        /// <param name="date">The balance date.</param>
        /// <param name="force">Defines if the balancing of every application should be forced.</param>
        /// <returns>The modified apps.</returns>
        public IEnumerable<Application> BalanceTelecollections(IEnumerable<Application> apps, DateTime date, bool force)
        {
            var computedApps = new List<Application>();
            var modifiedApps = new List<Application>();
            var tlcHours = new Dictionary<TimeSpan, int>();

            var maxCommerceCallsSpread = TimeSpan.FromMinutes(Options.MaxCommerceCallsSpreadMinutes);
            var delayBetweenCalls = TimeSpan.FromMinutes(Options.DelayBetweenCallsMinutes);

            TimeSpan GetLastValidStart(Range range)
            {
                var s = range.RangeStart;

                if (tlcHours.TryGetValue(range.RangeStart, out var kc) && kc >= Options.MaxCallPerMinute)
                {
                    var p = tlcHours
                        .Where(h =>
                            h.Key >= range.RangeStart &&
                            h.Key <= range.RangeEnd &&
                            h.Value < Options.MaxCallPerMinute
                        )
                        .OrderBy(h => h.Key)
                        .First();
                    s = p.Key;
                }

                return s;
            }

            // Grouping common telecollection ranges to balance them.
            foreach (var rangeGroup in apps
                .OrderBy(a => a.Contrat.Tpe.Commerce.Mcc.RangeStart)
                .GroupBy(a => new Range(a.Contrat.Tpe.Commerce.Mcc.RangeStart, a.Contrat.Tpe.Commerce.Mcc.RangeEnd)))
            {
                foreach (var item in Enumerable
                    .Range(0, (int)((rangeGroup.Key.RangeEnd - rangeGroup.Key.RangeStart).TotalMinutes) + 1)
                    .Select(m => rangeGroup.Key.RangeStart.Add(TimeSpan.FromMinutes(m))))
                {
                    tlcHours.TryAdd(item, 0);
                }

                foreach (var commerceGroup in rangeGroup
                             .OrderBy(g => g.Contrat.Tpe.Commerce.Identifiant).GroupBy(g => g.Contrat.Tpe.Commerce))
                {
                    // Getting the last valid start point for the commerce in the current range.
                    var commerceStartAt = GetLastValidStart(rangeGroup.Key);
                    //var commerceStartAt = rangeGroup.Key.RangeStart;
                    var commerceEndAt = commerceStartAt.Add(maxCommerceCallsSpread);
                    if (commerceEndAt > rangeGroup.Key.RangeEnd)
                    {
                        var t1 = rangeGroup.Key.RangeStart;
                        var t2 = rangeGroup.Key.RangeEnd.Subtract(maxCommerceCallsSpread);
                        commerceStartAt = t1 > t2 ? t1 : t2;
                        commerceEndAt = rangeGroup.Key.RangeEnd;
                    }

                    foreach (var tpeGroup in commerceGroup.GroupBy(c => c.Contrat.Tpe)
                                 .OrderBy(c => c.Key.NoSite)
                                 .ThenBy(c => c.Key.NoSerie))
                    {
                        // Getting the last valid start point for the terminal in the current range.
                        // If a commerce has more than 1 terminal and if a telecollection point is full, 
                        // we then need to take the next available one.
                        var tpeStart = GetLastValidStart(rangeGroup.Key);
                        //var tpeStart = commerceStartAt;

                        // If the TPE is wired on a RTC network, we set its TLC call time
                        // after the last RTC call time because if 2 communications overlap,
                        // they will both fail.
                        if (tpeGroup.Key.TypeConnexion == InfrastructureConstants.Tpe.RTC)
                        {
                            var lastRtcCall = computedApps
                                .Where(a =>
                                    a.Contrat.Tpe.Commerce == tpeGroup.Key.Commerce &&
                                    a.Contrat.Tpe.TypeConnexion == InfrastructureConstants.Tpe.RTC &&
                                    a.HeureTlc.HasValue
                                )
                                .Select(a => a.HeureTlc)
                                .OrderBy(a => a)
                                .Max();
                            if (tpeStart < lastRtcCall)
                            {
                                tpeStart = lastRtcCall.Value.Add(TimeSpan.FromMinutes(Options.DelayBetweenCallsMinutes));
                            }
                        }

                        foreach (var app in tpeGroup
                            .Where(a => !Options.ExcludedEquipment.Contains(a.Contrat.Tpe.Modele))             
                            .OrderBy(a => a.Contrat.Code, new ApplicationCodeComparer(Options.ApplicationPriorities))
                            .ThenBy(a => a.Contrat.NoContrat))
                        {
                            // If the terminal start point is outside the allowed commerce range, 
                            // we then loop the commerce start range to respect commerce
                            // telecollection spread constraint.
                            if (commerceEndAt < tpeStart)
                            {
                                tpeStart = commerceStartAt;
                            }

                            TimeSpan? heureTlc = null;

                            if (app.Contrat.DateFin is null ||
                                app.Contrat.DateFin.Value.Date > date.Date)
                            {
                                // If the telecollection point isn't full and the commerce telecollection spread constraint allows it,
                                // we shift the current app telecollection point to avoid call spikes.
                                while (commerceEndAt < tpeStart && tlcHours[tpeStart] >= Options.MaxCallPerMinute)
                                {
                                    tpeStart = tpeStart.Add(TimeSpan.FromMinutes(1));
                                }

                                heureTlc = tpeStart;
                                tlcHours[tpeStart] += 1;
                                tpeStart = tpeStart.Add(delayBetweenCalls);
                            }

                            if (app.HeureTlc != heureTlc || force)
                            {
                                modifiedApps.Add(app);
                            }
                            app.HeureTlc = heureTlc;
                            computedApps.Add(app);
                        }
                    }
                }
            }

            return modifiedApps;
        }

        private struct Range
        {
            public TimeSpan RangeStart { get; }

            public TimeSpan RangeEnd { get; }

            public Range(TimeSpan rangeStart, TimeSpan rangeEnd)
            {
                RangeStart = rangeStart;
                RangeEnd = rangeEnd;
            }

            public override bool Equals(object obj)
            {
                if (obj is Range r)
                {
                    return RangeStart == r.RangeStart && RangeEnd == r.RangeEnd;
                }

                return false;
            }

            public override int GetHashCode() => HashCode.Combine(RangeStart, RangeEnd);
        }
    }
}
