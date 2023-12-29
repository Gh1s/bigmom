using System.Collections.Generic;

namespace Csb.BigMom.Job
{
    /// <summary>
    /// Configuration options for the <see cref="DatabaseInitializationHostedService"/> hosted service.
    /// </summary>
    public class DatabaseInitializationOptions
    {
        /// <summary>
        /// SQL scripts to be executed after migrations.
        /// </summary>
        public IEnumerable<string> SqlScripts { get; set; }
    }
}
