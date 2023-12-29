using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Job.Balancing;
using Csb.BigMom.Job.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Job.IntegrationTests
{
    public class WebApplicationFactory : WebApplicationFactory<Startup>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Serilog:MinimumLevel:Default", Environment.GetEnvironmentVariable("BIGMOM_LOGLEVEL") ?? "Error" },
                    { "ConnectionStrings:BigMomContext", $"Host={Environment.GetEnvironmentVariable("BIGMOM_POSTGRES_HOST") ?? "localhost"};Database={DbName};Username=bigmom;Password=Pass@word1;Port=25432;IncludeErrorDetails=true" },
                    { "Kafka:Producer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Consumer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Integration.CommerceIntegrationRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Integration.TlcIntegrationRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.DataRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.DataResponse", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.BalanceRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.SpreadRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.SpreadResponse", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.CommerceIndexRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Spreading.TlcIndexRequest", Guid.NewGuid().ToString() },
                    { "Elasticsearch:Nodes:0", $"http://{Environment.GetEnvironmentVariable("BIGMOM_ELASTICSEARCH_HOST") ?? "localhost"}:9200" },
                    { "Elasticsearch:Indices:Csb.BigMom.Infrastructure.Entities.Commerce:Name", $"commerce-{Guid.NewGuid()}" },
                    { "Elasticsearch:Indices:Csb.BigMom.Infrastructure.Entities.Tlc:Name", $"tlc-{Guid.NewGuid()}" }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.Remove(services.Single(s => s.ServiceType == typeof(ISystemClock) && s.ImplementationType == typeof(SystemClock)));
                var systemClockMock = new Mock<ISystemClock>();
                systemClockMock.SetupGet(m => m.UtcNow).Returns(() => DateTimeOffset.UtcNow);
                services.AddSingleton(systemClockMock.Object);
                services.AddSingleton(systemClockMock);
                services.AddScoped<BalanceRequestHandler>();
                services.AddScoped<CommerceIndexRequestHandler>();
                services.AddConsumer<DataRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(DataRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.AddConsumer<SpreadRequest>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(SpreadRequest).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<CommerceIntegrationRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<TlcIntegrationRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<DataRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<DataResponse>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<BalanceRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<SpreadRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<SpreadResponse>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<CommerceIndexRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<TlcIndexRequest>)));
                services.PostConfigure<TraceOptions>(options => options.SerializationOptions.ReferenceHandler = ReferenceHandler.Preserve);
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

        public void PurgeData()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BigMomContext>();
            context.IntegrationTraces.RemoveRange(context.IntegrationTraces);
            context.Applications.RemoveRange(context.Applications);
            context.Tpes.RemoveRange(context.Tpes);
            context.Contrats.RemoveRange(context.Contrats);
            context.Commerces.RemoveRange(context.Commerces);
            context.Mccs.RemoveRange(context.Mccs);
            context.SaveChanges();
        }
    }
}
