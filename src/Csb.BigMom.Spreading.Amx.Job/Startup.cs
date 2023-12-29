using Confluent.Kafka;
using Csb.BigMom.Infrastructure;
using Csb.BigMom.Infrastructure.Spreading;
using Csb.BigMom.Spreading.Amx.Job.Spreading;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;

namespace Csb.BigMom.Spreading.Amx.Job
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
            // Kafka
            services.AddConsumer<SpreadRequest>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(SpreadRequest).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>();
            });
            services.AddProducer<SpreadResponse>(options =>
            {
                options.Topic = Configuration.GetSection($"Kafka:Topics:{typeof(SpreadResponse).FullName}").Get<string>();
                options.Config = Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>();
            });

            // MediatR
            // Using the IMediator as a markup type, avoids the auto registering of handlers.
            services.AddMediatR(typeof(IMediator));

            // Healthchecks
            services
                .AddHealthChecks()
                .AddKafka(
                    Configuration.GetSection("Kafka:Producer").Get<ProducerConfig>(),
                    Configuration.GetSection("Kafka:Topics:HealthChecks").Get<string>()
                );

            services.TryAddSingleton<ISystemClock, SystemClock>();

            // Spreading
            services.AddScoped<SpreadRequestValidator>();
            services.AddScoped<IPipelineBehavior<SpreadRequest, Unit>>(p => p.GetRequiredService<SpreadRequestValidator>());
            services.AddScoped<SpreadRequestHandler>();
            services.AddScoped<IPipelineBehavior<SpreadRequest, Unit>>(p => p.GetRequiredService<SpreadRequestHandler>());

            // Ace
            services.Configure<AceOptions>(Configuration.GetSection("Ace"));
            services.Configure<JobOptions>(Configuration.GetSection("Job"));
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
