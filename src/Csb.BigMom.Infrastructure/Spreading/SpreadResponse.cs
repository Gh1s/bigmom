using MediatR;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Spreading
{
    /// <summary>
    /// Models a spread response to a request from systems sent through Kafka.
    /// </summary>
    public class SpreadResponse : IRequest
    {
        /// <summary>
        /// The job that responded.
        /// </summary>
        [JsonPropertyName("job")]
        public string Job { get; set; }

        /// <summary>
        /// The status of the job's handling of the message.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// The error if the job failed to handle the spread request.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// The spread request trace ID.
        /// </summary>
        [JsonPropertyName("trace_identifier")]
        public string TraceIdentifier { get; set; }
    }
}
