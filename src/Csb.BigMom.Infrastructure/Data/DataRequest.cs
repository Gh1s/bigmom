using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Data
{
    /// <summary>
    /// Models a data request sent to spread jobs through Kafka.
    /// </summary>
    public class DataRequest
    {
        /// <summary>
        /// The ID of the spread trace.
        /// </summary>
        [JsonPropertyName("trace_identifier")]
        public string TraceIdentifier { get; set; }

        /// <summary>
        /// The job that should handle the request.
        /// </summary>
        [JsonPropertyName("job")]
        public string Job { get; set; }

        /// <summary>
        /// The parameters.
        /// </summary>
        [JsonPropertyName("params")]
        public Dictionary<string, string> Params { get; set; }

        /// <summary>
        /// The requested data.
        /// </summary>
        [JsonPropertyName("data")]
        public IEnumerable<string> Data { get; set; }
    }
}
