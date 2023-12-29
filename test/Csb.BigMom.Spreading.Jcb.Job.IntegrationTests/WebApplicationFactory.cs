using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Spreading.Jcb.Job;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csb.BigMom.Job.Spreading.Jcb.Job.IntegrationTests
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
                    { "Kafka:Producer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Consumer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Spreading.SpreadRequest", Guid.NewGuid().ToString() },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.Balancing.SpreadResponse", Guid.NewGuid().ToString() },
                    { "Ace:Host", Environment.GetEnvironmentVariable("BIGMOM_ACE_HOST") ?? "localhost" },
                    { "Ace:Port", "2222" },
                    { "Ace:Username", "bigmom" },
                    { "Ace:Password", "Pass@word1" }
                });
            });
            builder.ConfigureServices((ctx, services) =>
            {
                services.Remove(services.Single(s => s.ServiceType == typeof(ISystemClock) && s.ImplementationType == typeof(SystemClock)));
                var systemClockMock = new Mock<ISystemClock>();
                systemClockMock.SetupGet(m => m.UtcNow).Returns(() => DateTimeOffset.UtcNow);
                services.AddSingleton(systemClockMock.Object);
                services.AddSingleton(systemClockMock);
                services.AddConsumer<SpreadResponse>(options =>
                {
                    options.Topic = ctx.Configuration.GetSection($"Kafka:Topics:{typeof(SpreadResponse).FullName}").Get<string>();
                    options.Config = ctx.Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                });
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<SpreadRequest>)));
                services.Remove(services.Single(s => s.ServiceType == typeof(IHostedService) && s.ImplementationType == typeof(KafkaMessageHandler<SpreadResponse>)));
            });
        }
    }
}
