using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class Contrat
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("no_contrat")]
        public string NoContrat { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("date_debut")]
        public DateTime DateDebut { get; set; }

        [JsonPropertyName("date_fin")]
        public DateTime? DateFin { get; set; }

        [JsonPropertyName("tpe")]
        public Tpe Tpe { get; set; }

        [JsonPropertyName("applications")]
        public ICollection<Application> Applications { get; set; } = new HashSet<Application>();
    }
}
