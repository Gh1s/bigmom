using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    [ElasticsearchType(RelationName = "commerce")]
    public class Commerce
    {
        [JsonPropertyName("id"), JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonPropertyName("identifiant")]
        public string Identifiant { get; set; }

        [JsonPropertyName("mcc")]
        public Mcc Mcc { get; set; }

        [JsonPropertyName("nom")]
        public string Nom { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("ef")]
        public string Ef { get; set; }

        [JsonPropertyName("date_affiliation")]
        public DateTime DateAffiliation { get; set; }

        [JsonPropertyName("date_resiliation")]
        public DateTime? DateResiliation { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("tpes")]
        public ICollection<Tpe> Tpes { get; set; } = new HashSet<Tpe>();
    }
}
