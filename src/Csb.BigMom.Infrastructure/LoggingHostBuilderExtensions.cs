using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;

namespace Csb.BigMom.Infrastructure
{
    /// <summary>
    /// Provides extension methods <see cref="IHostBuilder"/>.
    /// </summary>
    public static class LoggingHostBuilderExtensions
    {
        /// <summary>
        /// Configures Serilog.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder) => builder
            .UseSerilog((context, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(context.Configuration);
                loggerConfig.Enrich.FromLogContext();
                loggerConfig.WriteTo.Console(theme: InfrastructureConstants.Logging.Theme, outputTemplate: InfrastructureConstants.Logging.OutputTemplate);

                if (!context.HostingEnvironment.IsDevelopment())
                {
                    var elasticsearchSinkOptions =
                        new ElasticsearchSinkOptions(new Uri(context.Configuration.GetValue<string>("Serilog:Elasticsearch:Url")))
                        {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = context.Configuration.GetValue<string>("Serilog:Elasticsearch:IndexFormat")
                        };
                    if (context.Configuration.GetValue<bool>("Serilog:Elasticsearch:BypassCertificateValidation"))
                    {
                        elasticsearchSinkOptions.ModifyConnectionSettings = configuration =>
                            configuration.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => true);
                    }
                    var assembly = Assembly.GetEntryAssembly();
                    loggerConfig.Enrich.WithProperty("Assembly", assembly.GetName().Name);
                    loggerConfig.Enrich.WithProperty("Version", $"{assembly.GetName().Version.Major}.{assembly.GetName().Version.Minor}.{assembly.GetName().Version.Build}");
                    loggerConfig.WriteTo.Elasticsearch(elasticsearchSinkOptions);
                }
            });
    }
}
