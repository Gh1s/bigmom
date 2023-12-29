using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Balancing;
using Csb.BigMom.Infrastructure.Data;
using Csb.BigMom.Infrastructure.Entities;
using Csb.BigMom.Infrastructure.Integration;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Job.Balancing;
using Csb.BigMom.Job.Data;
using Csb.BigMom.Job.Integration;
using Csb.BigMom.Job.Spreading;
using Elasticsearch.Net;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Csb.BigMom.Job
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // EF Core
            services.AddDbContext<BigMomContext>(builder =>
            {
                builder.UseNpgsql(Configuration.GetConnectionString("BigMomContext"));
                if (Environment.IsDevelopment())
                {
                    builder.EnableSensitiveDataLogging();
                    builder.EnableDetailedErrors();
                }
            });

            // Data initialization
            services.AddHostedService<DatabaseInitializationHostedService>();
            services.Configure<DatabaseInitializationOptions>(Configuration.GetSection("Initialization"));
            services.PostConfigure<DatabaseInitializationOptions>(options => options.SqlScripts ??= Array.Empty<string>());

            // Kafka
            services.AddConsumer<CommerceIntegrationRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(CommerceIntegrationRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
                options.ValueSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new CommerceIntegrationMccJsonConverter()
                    }
                };
            });
            services.AddConsumer<TlcIntegrationRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(TlcIntegrationRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<DataRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(DataRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<DataResponse>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(DataResponse).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<BalanceRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(BalanceRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<BalanceRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(BalanceRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<SpreadRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(SpreadRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<SpreadResponse>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(SpreadResponse).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<CommerceIndexRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(CommerceIndexRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<CommerceIndexRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(CommerceIndexRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<TlcIndexRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(TlcIndexRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });
            services.AddConsumer<TlcIndexRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(TlcIndexRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });

            // Elasticsearch
            services.AddSingleton<IConnectionPool>(
                new StaticConnectionPool(
                    Configuration.GetSection("Elasticsearch:Nodes")
                        .Get<string[]>()
                        .Select(n => new Uri(n))
                        .ToList()
                )
            );
            services.AddSingleton<IConnectionSettingsValues>(p =>
            {
                var settings = new ConnectionSettings(p.GetRequiredService<IConnectionPool>(),
                        (builtin, jsonSettings) => new JsonNetSerializer(
                            builtin,
                            jsonSettings,
                            () => new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            })
                    )
                    .DefaultMappingFor<Commerce>(m => m
                        .IndexName(Configuration.GetSection($"Elasticsearch:Indices:{typeof(Commerce).FullName}:Name").Get<string>())
                        .IdProperty(c => c.Id)
                    )
                    .DefaultMappingFor<Tlc>(m => m
                        .IndexName(Configuration.GetSection($"Elasticsearch:Indices:{typeof(Tlc).FullName}:Name").Get<string>())
                        .IdProperty(c => c.Id)
                    );
                if (Environment.IsDevelopment())
                {
                    settings.DisableAutomaticProxyDetection().EnableDebugMode();
                }
                return settings;
            });
            services.AddSingleton<IElasticClient>(p => new ElasticClient(p.GetRequiredService<IConnectionSettingsValues>()));
            services.AddHostedService<ElasticsearchIndexInitializationHostedService>();
            services.Configure<ElasticsearchOptions>(Configuration.GetSection("Elasticsearch"));

            // MediatR
            // Using the IMediator as a markup type, avoids the auto registering of handlers.
            services.AddMediatR(typeof(IMediator));

            // Healthchecks
            services
                .AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("BigMomContext"), name: "db")
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>()
                )
                .AddElasticsearch(options =>
                {
                    foreach (var uri in Configuration.GetSection("Elasticsearch:Nodes").Get<string[]>())
                    {
                        options.UseServer(uri);
                    }
                });

            services.TryAddSingleton<ISystemClock, SystemClock>();

            // Integration
            services.Configure<CommerceIntegrationOptions>(Configuration.GetSection("Integration:Commerce"));
            services.AddTransient<CommerceHashComputer>();
            services.AddScoped<CommerceIntegrationRequestValidator>();
            services.AddScoped<IPipelineBehavior<CommerceIntegrationRequest, Unit>>(p => 
                p.GetRequiredService<CommerceIntegrationRequestValidator>());
            services.AddScoped<CommerceIntegrationRequestNormalizer>();
            services.AddScoped<IPipelineBehavior<CommerceIntegrationRequest, Unit>>(p => 
                p.GetRequiredService<CommerceIntegrationRequestNormalizer>());
            services.AddScoped<CommerceIntegrationRequestHandler>();
            services.AddScoped<IPipelineBehavior<CommerceIntegrationRequest, Unit>>(p => 
                p.GetRequiredService<CommerceIntegrationRequestHandler>());
            services.AddScoped<CommerceIntegrationRequestNotifier>();
            services.AddScoped<IPipelineBehavior<CommerceIntegrationRequest, Unit>>(p => 
                p.GetRequiredService<CommerceIntegrationRequestNotifier>());
            services.Configure<TlcIntegrationOptions>(Configuration.GetSection("Integration:Tlc"));
            services.AddScoped<TlcIntegrationValidator>();
            services.AddScoped<IPipelineBehavior<TlcIntegrationRequest, Unit>>(p =>
                p.GetRequiredService<TlcIntegrationValidator>());
            services.AddScoped<TlcIntegrationHandler>();
            services.AddScoped<IPipelineBehavior<TlcIntegrationRequest, Unit>>(p =>
                p.GetRequiredService<TlcIntegrationHandler>());
            services.AddScoped<TlcIntegrationRequestNotifier>();
            services.AddScoped<IPipelineBehavior<TlcIntegrationRequest, Unit>>(p =>
                p.GetRequiredService<TlcIntegrationRequestNotifier>());

            // Data
            services.AddScoped<CommerceIndexRequestHandler>();
            services.AddScoped<IPipelineBehavior<CommerceIndexRequest, Unit>>(p => p.GetRequiredService<CommerceIndexRequestHandler>());
            services.AddScoped<TlcIndexRequestHandler>();
            services.AddScoped<IPipelineBehavior<TlcIndexRequest, Unit>>(p => p.GetRequiredService<TlcIndexRequestHandler>());
            services.Configure<DataRequestOptions>(Configuration.GetSection("Data"));
            services.AddScoped<DataResponseHandler>();
            services.AddScoped<IPipelineBehavior<DataResponse, Unit>>(p => p.GetRequiredService<DataResponseHandler>());

            // Balancing
            services.AddTransient<Balancer>();
            services.Configure<BalancerOptions>(Configuration.GetSection("Balancing"));
            services.AddScoped<BalanceRequestHandler>();
            services.AddScoped<IPipelineBehavior<BalanceRequest, Unit>>(p => p.GetRequiredService<BalanceRequestHandler>());

            // Spreading
            services.Configure<SpreadOptions>(Configuration.GetSection("Spreading"));
            services.PostConfigure<SpreadOptions>(options => options.IncludedCommerces ??= Enumerable.Empty<string>());
            services.AddScoped<SpreadBatchCommandHandler>();
            services.AddScoped<IRequestHandler<SpreadBatchCommand, Unit>>(p => p.GetRequiredService<SpreadBatchCommandHandler>());
            services.AddScoped<SpreadResponseHandler>();
            services.AddScoped<IPipelineBehavior<SpreadResponse, Unit>>(p => p.GetRequiredService<SpreadResponseHandler>());

            services.PostConfigure<TraceOptions>(options =>
            {
                options.SerializationOptions ??= new();
                options.SerializationOptions.Converters.Add(new TimeSpanJsonConverter());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health");
        }
    }
}
