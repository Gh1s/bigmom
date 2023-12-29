using Csb.BigMom.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Api.IntegrationTests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        // These access_token are fake ones. They're never introspected by the IDP as we mock its response.

        public string AccessToken { get; } = Guid.NewGuid().ToString();

        public string InactiveAccessToken { get; } = Guid.NewGuid().ToString();

        public string FailedAccessToken { get; } = Guid.NewGuid().ToString();

        public Mock<HttpMessageHandler> IdpMessageHandlerMock { get; } = new Mock<HttpMessageHandler>();

        public WebApplicationFactory()
        {
            IdpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns<HttpRequestMessage, CancellationToken>(async (message, cancellationToken) =>
                {
                    HttpResponseMessage response;

                    if (message.RequestUri.ToString().EndsWith("/oauth2/introspect"))
                    {
                        string accessToken = null;

                        if (message.Content is FormUrlEncodedContent content)
                        {
                            var formData = QueryHelpers.ParseQuery(await message.Content.ReadAsStringAsync(CancellationToken.None));
                            if (formData.TryGetValue("token", out var tokenValues))
                            {
                                accessToken = tokenValues.ToString();
                            }
                        }

                        if (accessToken == FailedAccessToken)
                        {
                            throw new InvalidOperationException("This is a failing access token");
                        }

                        var token = new
                        {
                            active = accessToken == AccessToken,
                            sub = TestConstants.User.Id,
                            client_id = "bigmom",
                            ext = new
                            {
                                preferred_username = TestConstants.User.Username,
                                given_name = TestConstants.User.FirstName,
                                family_name = TestConstants.User.LastName,
                                name = $"{TestConstants.User.LastName} {TestConstants.User.FirstName}"
                            }
                        };
                        response = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(
                                JsonSerializer.Serialize(token),
                                Encoding.UTF8,
                                MediaTypeNames.Application.Json
                            )
                        };
                    }
                    else
                    {
                        response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }

                    return response;
                })
                .Verifiable();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Serilog:MinimumLevel:Default", Environment.GetEnvironmentVariable("BIGMOM_LOGLEVEL") ?? "Error" },
                    { "ConnectionStrings:BigMomContext", $"Host={Environment.GetEnvironmentVariable("BIGMOM_POSTGRES_HOST") ?? "localhost"};Database={DbName};Username=bigmom;Password=Pass@word1;Port=25432;IncludeErrorDetails=true" },
                    { "Kafka:Producer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Elasticsearch:Nodes:0", $"http://{Environment.GetEnvironmentVariable("BIGMOM_ELASTICSEARCH_HOST") ?? "localhost"}:9200" },
                    { "Elasticsearch:Indices:Csb.BigMom.Infrastructure.Entities.Commerce:Name", $"comemrce-{Guid.NewGuid()}" },
                    { "Elasticsearch:Indices:Csb.BigMom.Infrastructure.Entities.Tlc:Name", $"tlc-{Guid.NewGuid()}" }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.AddHttpClient(IntrospectionOptions.HttpClient).ConfigurePrimaryHttpMessageHandler(() => IdpMessageHandlerMock.Object);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BigMomContext>();
            context.Database.Migrate();
            return host;
        }

        public HttpClient CreateClientWithAuthentication()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(IntrospectionOptions.Scheme, AccessToken);
            return client;
        }
    }
}
