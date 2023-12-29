using System.Text.Json.Serialization;
using Csb.BigMom.Infrastructure.Entities;
using MediatR;
using Newtonsoft.Json;

namespace Csb.BigMom.Infrastructure.Integration
{
    /// <summary>
    /// Models a commerce integration request sent through Kafka.
    /// </summary>
    public class CommerceIntegrationRequest : IRequest
    {
        /// <summary>
        /// The unique identifier pour the batch.
        /// </summary>
        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        /// <summary>
        /// The message index in the batch.
        /// </summary>
        [JsonPropertyName("index")]
        public long Index { get; set; }

        /// <summary>
        /// The total number of messages in the batch.
        /// </summary>
        [JsonPropertyName("total")]
        public long Total { get; set; }

        /// <summary>
        /// The payload.
        /// </summary>
        [JsonPropertyName("commerce")]
        public Commerce Commerce { get; set; }

        /// <summary>
        /// The integration trace.
        /// </summary>
        public CommerceIntegrationTrace Trace { get; set; }
    }
}
