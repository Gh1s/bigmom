using Csb.BigMom.Infrastructure.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides a hosted service to intialize Elasticsearch indices.
    /// </summary>
    public class ElasticsearchIndexInitializationHostedService : IHostedService
    {
        private readonly IElasticClient _client;
        private readonly IOptions<ElasticsearchOptions> _options;
        private readonly ILogger<ElasticsearchIndexInitializationHostedService> _logger;

        public ElasticsearchIndexInitializationHostedService(
            IElasticClient client,
            IOptions<ElasticsearchOptions> options,
            ILogger<ElasticsearchIndexInitializationHostedService> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken) => Task.WhenAll(
            SetupCommerceIndex(cancellationToken),
            SetupTlcIndex(cancellationToken)
        );

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task SetupCommerceIndex(CancellationToken cancellationToken)
        {
            var index = _options.Value.GetIndexOptions<Commerce>().Name;
            _logger.LogInformation("Creating the index template: {0}", index);

            var response = await _client.Indices.PutTemplateAsync(
                index,
                desc => desc
                    .IndexPatterns(index + "*")
                    .Settings(settings => settings
                        .Analysis(a => a
                            .Tokenizers(t => t
                                .EdgeNGram("ngram", g => g.MinGram(3).MaxGram(30))
                            )
                            .Analyzers(al => al
                                .Custom("ngram", c => c
                                    .Tokenizer("ngram")
                                    .Filters("asciifolding", "lowercase")
                                )
                            )
                        )
                    )
                    .Map<Commerce>(map => map
                        .AutoMap<Commerce>()
                        .Properties(prop => prop
                            .Object<Mcc>(mcc => mcc
                                .Name(n => n.Mcc)
                                .Properties(prop5 => prop5
                                    .Keyword(p => p.Name(n => n.Code))
                                )
                            )
                            .Text(p => p
                                .Name(n => n.Identifiant)
                                .Analyzer("ngram")
                                .SearchAnalyzer("standard")
                                .Fields(f => f.Keyword(k => k.Name("keyword")))
                            )
                            .Text(p => p
                                .Name(n => n.Nom)
                                .Analyzer("ngram")
                                .SearchAnalyzer("standard")
                                .Fields(f => f.Keyword(k => k.Name("keyword")))
                            )
                            .Text(p => p
                                .Name(n => n.Email)
                                .Analyzer("ngram")
                                .SearchAnalyzer("standard")
                                .Fields(f => f.Keyword(k => k.Name("keyword")))
                            )
                            .Keyword(p => p.Name(n => n.Ef))
                            .Keyword(p => p.Name(n => n.Hash))
                        )
                    ),
                cancellationToken
            );

            if (response.IsValid)
            {
                _logger.LogInformation("Index {0} created.", index);
            }
            else
            {
                _logger.LogError(response.OriginalException, "An error has occured while creating the index {0}.", index);
                _logger.LogDebug(response.DebugInformation);
            }
        }

        private async Task SetupTlcIndex(CancellationToken cancellationToken)
        {
            var index = _options.Value.GetIndexOptions<Tlc>().Name;
            _logger.LogInformation("Creating the index template: {0}", index);

            var response = await _client.Indices.PutTemplateAsync(
                index,
                desc => desc
                    .IndexPatterns(index + "*")
                    .Settings(settings => settings
                        .Analysis(a => a
                            .Tokenizers(t => t
                                .EdgeNGram("ngram", g => g.MinGram(3).MaxGram(30))
                            )
                            .Analyzers(al => al
                                .Custom("ngram", c => c
                                    .Tokenizer("ngram")
                                    .Filters("asciifolding", "lowercase")
                                )
                            )
                        )
                    )
                    .Map<Tlc>(map => map
                        .AutoMap<Tlc>()
                        .Properties(prop => prop
                            .Keyword(p => p.Name(n => n.Status))
                            .Object<Application>(app => app
                                .Name(n => n.App)
                                .Properties(prop2 => prop2
                                    .Text(p => p
                                        .Name(n => n.Idsa)
                                        .Analyzer("ngram")
                                        .SearchAnalyzer("standard")
                                        .Fields(f => f.Keyword(k => k.Name("keyword")))
                                    )
                                    
                                    .Object<Contrat>(contrat => contrat
                                        .Name(n => n.Contrat)
                                        .Properties(prop3 => prop3
                                            .Text(p => p
                                                .Name(n => n.NoContrat)
                                                .Analyzer("ngram")
                                                .SearchAnalyzer("standard")
                                                .Fields(f => f.Keyword(k => k.Name("keyword")))
                                            )
                                            .Text(p => p
                                                .Name(n => n.Code)
                                                .Analyzer("ngram")
                                                .SearchAnalyzer("standard")
                                                .Fields(f => f.Keyword(k => k.Name("keyword")))
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                cancellationToken
            );

            if (response.IsValid)
            {
                _logger.LogInformation("Index {0} created.", index);
            }
            else
            {
                _logger.LogError(response.OriginalException, "An error has occured while creating the index {0}.", index);
                _logger.LogDebug(response.DebugInformation);
            }
        }
    }
}
