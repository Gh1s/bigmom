using System.Collections.Generic;

namespace Csb.BigMom.Job.Data
{
    /// <summary>
    /// Configuration options for the <see cref="DataRequest"/>.
    /// </summary>
    public class DataRequestOptions
    {
        /// <summary>
        /// The requests configuration.
        /// </summary>
        public Dictionary<string, DataRequestConfig> Config { get; set; }
    }

    /// <summary>
    /// Models a <see cref="DataRequest"/> configuration.
    /// </summary>
    public class DataRequestConfig
    {
        /// <summary>
        /// The job that should handle the request.
        /// </summary>
        public string Job { get; set; }

        /// <summary>
        /// The requested data.
        /// </summary>
        public IEnumerable<string> Data { get; set; }
    }
}
