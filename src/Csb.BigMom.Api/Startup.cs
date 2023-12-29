using Confluent.Kafka;
using Csb.BigMom.Api.Models;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Entities;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Csb.BigMom.Api
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
            // Cors
            services.AddCors(options =>
            {
                var section = Configuration.GetSection("Cors");
                options.AddDefaultPolicy(p => p
                    .AllowAnyHeader()
                    .WithOrigins(section.GetSection("AllowedOrigins").Get<string[]>())
                    .WithMethods(section.GetSection("AllowedMethods").Get<string[]>())
                );
            });

            // Mvc
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
                    options.JsonSerializerOptions.Converters.Add(new ISearchFilterJsonConverter());
                });

            // EF Core
            services.AddDbContext<BigMomContext>(builder => builder.UseNpgsql(Configuration.GetConnectionString("BigMomContext")));

            // Security.
            // Clearing the default claims map to avoid claim missmap.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services
                .AddAuthentication(IntrospectionOptions.Scheme)
                .AddScheme<IntrospectionOptions, IntrospectionHandler>(
                    IntrospectionOptions.Scheme,
                    options => Configuration.GetSection("Authentication").Bind(options)
                );
            // Introspection.
            services.AddHttpClient(IntrospectionOptions.HttpClient, client =>
                client.BaseAddress = new Uri(Configuration.GetValue<string>("Authentication:Authority")));
            // Our default authorization policy requires user to be authenticated.
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
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

            // Healthchecks.
            services.AddHealthChecks()
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UsePathBase(Configuration.GetValue<string>("PathBase"));

            app.UseHealthChecks("/health");

            app.UseRouting();

            app.UseCors();

            if (Configuration.GetSection("Authentication:Enabled").Get<bool>())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
