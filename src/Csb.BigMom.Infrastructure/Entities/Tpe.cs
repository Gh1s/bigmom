using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    public class Tpe
    {
        [JsonPropertyName("id"),]
        public int Id { get; set; }

        [JsonPropertyName("no_serie")]
        public string NoSerie { get; set; }

        [JsonPropertyName("no_site")]
        public string NoSite { get; set; }

        [JsonPropertyName("modele")]
        public string Modele { get; set; }

        [JsonPropertyName("statut")]
        public string Statut { get; set; }

        [JsonPropertyName("type_connexion")]
        public string TypeConnexion { get; set; }

        [JsonPropertyName("commerce")]
        public Commerce Commerce { get; set; }

        [JsonPropertyName("contrats")]
        public ICollection<Contrat> Contrats { get; set; } = new HashSet<Contrat>();

        /*[JsonPropertyName("applications")]
        public ICollection<Application> Applications { get; set; } = new HashSet<Application>();*/
    }
}
