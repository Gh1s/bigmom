using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.TestCommon;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Csb.BigMom.Api.IntegrationTests.Controller
{
    public class TlcControllerTests : IClassFixture<WebApplicationFactory>
    {
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new TimeSpanJsonConverter()
            }
        };
        private readonly TestData _data = new();
        private readonly WebApplicationFactory _factory;
        private readonly IElasticClient _elasticClient;

        public TlcControllerTests(WebApplicationFactory factory)
        {
            _factory = factory;
            _elasticClient = factory.Services.GetRequiredService<IElasticClient>();
        }

        [Fact]
        public async Task Search_Test()
        {
            //Setup
            IndexData();

            var search = new
            {
                search = "TPE01",
                skip = 0,
                take = 100
            };
            var client = _factory.CreateClientWithAuthentication();

            //Act
            var result = await client.PostAsJsonAsync("/tlcs", search);

            //Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            var expectedTlcs = _data.Tlcs.Where(t => t.App.Tpe.NoSerie.StartsWith("TPE01")).OrderBy(i => i.Id).ToList();
            var json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Models.SearchResponse<Tlc>>(json, _serializerOptions);
            response.Skip.Should().Be(0);
            response.Take.Should().Be(expectedTlcs.Count);
            response.Total.Should().Be(expectedTlcs.Count);
            response
                .Results
                .OrderBy(i => i.Id)
                .Should()
                .BeEquivalentTo(
                    expectedTlcs,
                    options => options
                        .IgnoringCyclicReferences()
                        .Excluding(m => m.App.Tpe.Applications)
                        .Excluding(m => m.App.Tpe.Commerce.Tpes)
                        .Excluding(m => m.App.Tpe.Commerce.Contrats)
                        .Excluding(m => m.App.Contrat.Applications)
                );

        }

        [Fact]
        public async Task Search_WithFilters_Test()
        {
            //Setup
            IndexData();

            var search = new
            {
                filters = new
                {
                    processing_date = new
                    {
                        start = "2021-01-01T21:10",
                        end = "2021-01-01T21:14"
                    },
                    status = new[]
                    {
                        "KO"
                    }
                },
                take = 100
            };
            var client = _factory.CreateClientWithAuthentication();

            //Act
            var result = await client.PostAsJsonAsync("/tlcs", search);

            //Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            var expectedTlcs = _data.Tlcs.FindAll(t =>
                t.Status == "KO" &&
                t.ProcessingDate >= new DateTimeOffset(2021, 1, 1, 21, 10, 0, DateTimeOffset.UtcNow.ToLocalTime().Offset) &&
                t.ProcessingDate <= new DateTimeOffset(2021, 1, 1, 21, 14, 0, DateTimeOffset.UtcNow.ToLocalTime().Offset)
            );
            var json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Models.SearchResponse<Tlc>>(json, _serializerOptions);
            response.Skip.Should().Be(0);
            response.Take.Should().Be(expectedTlcs.Count);
            response.Total.Should().Be(expectedTlcs.Count);
            response.Results.Should().BeEquivalentTo(expectedTlcs);
        }

        [Fact]
        public async Task Search_WithSorts_Test()
        {
            //Setup
            IndexData();

            var search = new
            {
                skip = 0,
                Take = 100,
                filters = new Dictionary<string, object>
                {
                    { "app.tpe.no_serie", new[]{ "TPE001", "TPE002" } }
                },
                sorts = new Dictionary<string, string>
                {
                    { "app.tpe.no_serie", "desc" },
                    { "processing_date", "desc" }
                }
            };

            var client = _factory.CreateClientWithAuthentication();

            //Act
            var result = await client.PostAsJsonAsync("/tlcs", search);

            //Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            var expectedTlcs = _data.Tlcs
                .Where(t => t.App.Tpe.NoSerie == "TPE001" || t.App.Tpe.NoSerie == "TPE002")
                .OrderByDescending(t => t.App.Tpe.NoSerie)
                .ThenByDescending(t => t.ProcessingDate)
                .ToList();
            var json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Models.SearchResponse<Tlc>>(json, _serializerOptions);
            response.Skip.Should().Be(0);
            response.Take.Should().Be(expectedTlcs.Count);
            response.Total.Should().Be(expectedTlcs.Count);
            response.Results.Should().BeEquivalentTo(expectedTlcs);
        }

        [Fact]
        public async Task Search_WithLimitedResult_Test()
        {
            //Setup
            var skip = 5;
            var take = 5;
            IndexData();

            var search = new
            {
                skip = skip,
                take = take
            };
            var client = _factory.CreateClientWithAuthentication();

            //Act
            var result = await client.PostAsJsonAsync("/tlcs", search);

            //Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            var expectedTlcs = _data.Tlcs.GetRange(skip, take);
            var json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Models.SearchResponse<Tlc>>(json, _serializerOptions);
            response.Skip.Should().Be(skip);
            response.Take.Should().Be(take);
            response.Total.Should().Be(_data.Tlcs.Count);
            response.Results.Should().BeEquivalentTo(expectedTlcs);
        }

        [Fact]
        public async Task Search_WithAggregations_Test()
        {
            //Setup
            IndexData();

            var search = new
            {
                take = 5,
                aggregations = new[]
                {
                    "status",
                    "app.tpe.statut",
                    "app.contrat.code"
                }
            };
            var client = _factory.CreateClientWithAuthentication();

            //Act
            var result = await client.PostAsJsonAsync("/tlcs", search);

            //Assert
            result.IsSuccessStatusCode.Should().BeTrue();
            var expectedTlcs = _data.Tlcs.GetRange(0, 5);
            var json = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Models.SearchResponse<Tlc>>(json, _serializerOptions);
            response.Skip.Should().Be(0);
            response.Take.Should().Be(5);
            response.Total.Should().Be(_data.Tlcs.Count);
            response.Results.Should().BeEquivalentTo(expectedTlcs);
            response.Aggregations["status"].Cast<JsonElement>().Select(e => e.GetString()).Should().BeEquivalentTo(new[] { "KO", "OK" });
            response.Aggregations["app.tpe.statut"].Cast<JsonElement>().Select(e => e.GetString()).Should().BeEquivalentTo(new[] { "Actif", "Inactif" });
            response.Aggregations["app.contrat.code"].Cast<JsonElement>().Select(e => e.GetString()).Should().BeEquivalentTo(new[] { "EMV", "NFC", "JADE", "VAD" });
        }

        private void IndexData()
        {
            var i = 0;
            _data.Tlcs.ForEach(t =>
            {
                t.Id = ++i;
                t.App.Tlcs = null;
                t.App.Tpe.Applications = null;
                t.App.Tpe.Commerce.Tpes = null;
                t.App.Tpe.Commerce.Contrats = null;
                t.App.Contrat.Applications = null;
            });
            var bulkResponse = _elasticClient.Bulk(b =>
            {
                foreach (var tlc in _data.Tlcs)
                {
                    b.Index<Tlc>(d => d.Document(tlc));
                }

                b.Refresh(Refresh.True);
                return b;
            });
        }
    }
}