using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Csb.BigMom.Infrastructure.IntegrationTests
{
    public class Config
    {
        public static IConfiguration BuildConfiguration(Action<IConfigurationBuilder> configureAction = null)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:BigMomContext", $"Host={Environment.GetEnvironmentVariable("BIGMOM_POSTGRES_HOST") ?? "localhost"};Database={Guid.NewGuid()};Username=bigmom;Password=Pass@word1;Port=25432;IncludeErrorDetails=true" },
                    { "Kafka:Producer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Consumer:BootstrapServers", $"{Environment.GetEnvironmentVariable("BIGMOM_KAFKA_HOST") ?? "localhost"}:9092" },
                    { "Kafka:Topics:Csb.BigMom.Infrastructure.IntegrationTests.TestMessage", Guid.NewGuid().ToString() },
                    { "Kafka:Consumer:GroupId", "test" },
                    { "Kafka:Consumer:AutoOffsetReset", "Earliest" },
                    { "Kafka:Consumer:EnablePartitionEof", "true" },
                    { "Kafka:Consumer:EnableAutoCommit", "false" },
                    { "Kafka:Consumer:AllowAutoCreateTopics", "true" },
                    { "Elasticsearch:Nodes:0", $"http://{Environment.GetEnvironmentVariable("BIGMOM_ELASTICSEARCH_HOST") ?? "localhost"}:9200" },
                    { "Elasticsearch:Indices:Csb.BigMom.Infrastructure.Entities.Commerce", Guid.NewGuid().ToString() }
                });
            configureAction?.Invoke(configBuilder);
            return configBuilder.Build();
        }
    }
}
