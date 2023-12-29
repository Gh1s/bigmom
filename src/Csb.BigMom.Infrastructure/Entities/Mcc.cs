using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class Mcc
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("range_start")]
        public TimeSpan RangeStart { get; set; }

        [JsonPropertyName("range_end")]
        public TimeSpan RangeEnd { get; set; }
    }
}
