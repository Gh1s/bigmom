using Csb.BigMom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.Job
{
    /// <summary>
    /// Provides an hosted service to initialize the database.
    /// </summary>
    public sealed class DatabaseInitializationHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScope _scope;
        private IOptions<DatabaseInitializationOptions> _options;
        private readonly ILogger<DatabaseInitializationHostedService> _logger;

        private IServiceProvider Provider => _scope.ServiceProvider;

        private DatabaseInitializationOptions Options => _options.Value;

        public DatabaseInitializationHostedService(
            IServiceProvider provider,
            IOptions<DatabaseInitializationOptions> options,
            ILogger<DatabaseInitializationHostedService> logger)
        {
            _scope = provider.CreateScope();
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Dispose() => _scope.Dispose();

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting data initialization.");

            var context = Provider.GetRequiredService<BigMomContext>();
            await context.Database.MigrateAsync(cancellationToken);

            foreach (var sqlScript in Options.SqlScripts)
            {
                _logger.LogInformation("Executing SQL script: {0}", sqlScript);

                try
                {
                    await context.Database.ExecuteSqlRawAsync(File.ReadAllText(sqlScript), cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error has occured while executing SQL script: {0}", sqlScript);
                }
            }
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ending data initialization.");
            return Task.CompletedTask;
        }
    }
}
