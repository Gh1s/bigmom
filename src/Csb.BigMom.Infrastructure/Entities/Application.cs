using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class Application
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("heure_tlc")]
        public TimeSpan? HeureTlc { get; set; }

        [JsonPropertyName("idsa")]
        public string Idsa { get; set; }

        /*[JsonPropertyName("tpe")]
        public Tpe Tpe { get; set; }*/

        [JsonPropertyName("contrat")]
        public Contrat Contrat { get; set; }

        [JsonPropertyName("tlcs")]
        public ICollection<Tlc> Tlcs { get; set; } = new HashSet<Tlc>();
    }
}
