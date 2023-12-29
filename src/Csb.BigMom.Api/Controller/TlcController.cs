using Csb.BigMom.Api.Models;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Csb.BigMom.Api.Controller
{
    public class TlcController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;
        private readonly IOptionsSnapshot<ElasticsearchOptions> _optionsSnapshot;
        private readonly ILogger<TlcController> _logger;

        private ElasticsearchOptions Options => _optionsSnapshot.Value;

        public TlcController(
            IElasticClient elasticClient,
            IOptionsSnapshot<ElasticsearchOptions> optionsSnapshot,
            ILogger<TlcController> logger)
        {
            _elasticClient = elasticClient;
            _optionsSnapshot = optionsSnapshot;
            _logger = logger;
        }

        [HttpGet("/tlcs/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _logger.LogInformation("Getting the TLC {0}.", id);

            var response = await _elasticClient.GetAsync<Tlc>(id);
            if (response.IsValid)
            {
                _logger.LogInformation("TLC found.");
                return Ok(response.Source);
            }
            else
            {
                _logger.LogWarning("TLC not found.");
                return NoContent();
            }
        }

        [HttpGet("/tlcs"), HttpPost("/tlcs")]
        public async Task<Models.SearchResponse<Tlc>> Search([FromBody] Models.SearchRequest request)
        {
            _logger.LogInformation("Executing a TLC search request.");
            _logger.LogTraceObject("Request", request);

            var indexOptions = Options.GetIndexOptions<Tlc>();
            var response = await _elasticClient.SearchAsync<Tlc>(s => s
                .Query(q => q
                    .Bool(b =>
                    {
                        if (!string.IsNullOrWhiteSpace(request.Search))
                        {
                            b.Should(
                                sh => sh.Match(p => p.Field(f => f.App.Idsa).Query(request.Search).Boost(10)),
                                sh => sh.Match(p => p.Field(f => f.App.Contrat.Tpe.NoSerie).Query(request.Search).Boost(10)),
                                sh => sh.Match(p => p.Field(f => f.App.Contrat.Tpe.Commerce.Identifiant).Query(request.Search).Boost(5)),
                                sh => sh.Match(p => p.Field(f => f.App.Contrat.Tpe.Commerce.Nom).Query(request.Search)),
                                sh => sh.Match(p => p.Field(f => f.App.Contrat.NoContrat).Query(request.Search).Boost(2)),
                                sh => sh.Match(p => p.Field(f => f.App.Contrat.Code).Query(request.Search).Boost(2))
                            );
                            b.MinimumShouldMatch(1);
                        }

                        if (request.Filters != null)
                        {
                            var filters = new List<Func<QueryContainerDescriptor<Tlc>, QueryContainer>>();

                            foreach (var filter in request.Filters)
                            {
                                var key = filter.Key;
                                if (indexOptions.Fields.TryGetValue(key, out var field))
                                {
                                    key = field;
                                }

                                if (filter.Value is ArraySearchFilter<string> stringValues)
                                {
                                    filters.Add(fl => fl.Terms(t => t.Field(key).Terms(stringValues.Values)));
                                }
                                else if (filter.Value is ArraySearchFilter<double> numericValues)
                                {
                                    filters.Add(fl => fl.Terms(t => t.Field(key).Terms(numericValues.Values)));
                                }
                                else if (filter.Value is RangeSearchFilter<DateTimeOffset> dateRange)
                                {
                                    filters.Add(fl => fl.DateRange(r => r.Field(key).GreaterThanOrEquals(DateMath.Anchored(dateRange.Start.UtcDateTime)).LessThanOrEquals(DateMath.Anchored(dateRange.End.UtcDateTime))));
                                }
                                else if (filter.Value is RangeSearchFilter<double> numericRange)
                                {
                                    filters.Add(fl => fl.Range(r => r.Field(key).GreaterThanOrEquals(numericRange.Start).LessThanOrEquals(numericRange.End)));
                                }
                            }

                            b.Filter(filters);
                        }
                        return b;
                    })
                )
                .Skip(request.Skip)
                .Take(request.Take)
                .Aggregations(a =>
                {
                    if (request.Aggregations != null)
                    {
                        foreach (var agg in request.Aggregations)
                        {
                            var key = agg;
                            if (indexOptions.Fields.TryGetValue(key, out var field))
                            {
                                key = field;
                            }

                            a.Terms(agg, p => p.Field(key));
                        }
                    }
                    return a;
                })
                .Sort(s =>
                {
                    SortDescriptor<Tlc> desc = s;

                    if (request.Sorts != null)
                    {
                        foreach (var sort in request.Sorts)
                        {
                            var key = sort.Key;
                            if (indexOptions.Fields.TryGetValue(key, out var field))
                            {
                                key = field;
                            }

                            if (string.Equals(sort.Value, "asc", StringComparison.OrdinalIgnoreCase))
                            {
                                desc = desc.Ascending(key);
                            }
                            else
                            {
                                desc = desc.Descending(key);
                            }
                        }
                    }
                    return s;
                })
            );

            if (response.IsValid)
            {
                _logger.LogInformation("Request execution succeeded.");
                return new()
                {
                    Skip = request.Skip,
                    Take = response.Documents.Count,
                    Total = response.Total,
                    Results = response.Documents,
                    Aggregations = request.Aggregations?.ToDictionary(
                        a => a,
                        b =>
                        {
                            if (response.Aggregations[b] is BucketAggregate bucket)
                            {
                                return bucket.Items.Select(i => (i as KeyedBucket<object>).Key).OrderBy(i => i).ToList();
                            }
                            return Enumerable.Empty<object>();
                        }
                    )
                };
            }
            else
            {
                _logger.LogWarning(response.OriginalException, "Request execution failed.");
                _logger.LogDebug(response.DebugInformation);
                return new()
                {
                    Results = Enumerable.Empty<Tlc>()
                };
            }
        }
    }
}