using MediatR;
using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Spreading
{
    /// <summary>
    /// Models a spread request to other systems sent through Kafka.
    /// </summary>
    public class SpreadRequest : IRequest
    {
        /// <summary>
        /// The ID of the spread trace.
        /// </summary>
        [JsonPropertyName("trace_identifier")]
        public string TraceIdentifier { get; set; }

        /// <summary>
        /// The commerce id.
        /// </summary>
        [JsonPropertyName("commerce_id")]
        public string CommerceId { get; set; }

        /// <summary>
        /// The serial number of the terminal.
        /// </summary>
        [JsonPropertyName("no_serie_tpe")]
        public string NoSerieTpe { get; set; }

        /// <summary>
        /// The site number of the terminal.
        /// </summary>
        [JsonPropertyName("no_site_tpe")]
        public string NoSiteTpe { get; set; }

        /// <summary>
        /// The application contract number.
        /// </summary>
        [JsonPropertyName("no_contrat")]
        public string NoContrat { get; set; }

        /// <summary>
        /// The application code.
        /// </summary>
        [JsonPropertyName("application_code")]
        public string ApplicationCode { get; set; }

        /// <summary>
        /// The telecollection hour.
        /// </summary>
        [JsonPropertyName("heure_tlc")]
        public DateTime? HeureTlc { get; set; }

        /// <summary>
        /// The bank code.
        /// </summary>
        [JsonPropertyName("ef")]
        public string Ef { get; set; }
    }
}
