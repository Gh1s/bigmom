using Nest;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Entities
{
    [ElasticsearchType(RelationName = "tlc")]
    public class Tlc
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("processing_date")]
        public DateTimeOffset ProcessingDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("nbr_debit")]
        public int NbTransactionsDebit { get; set; }

        [JsonPropertyName("total_debit")]
        public decimal TotalDebit { get; set; }

        [JsonPropertyName("nbr_credit")]
        public int NbTransactionsCredit { get; set; }

        [JsonPropertyName("total_credit")]
        public decimal TotalCredit { get; set; }

        [JsonPropertyName("total_reconcilie")]
        public decimal TotalReconcilie { get; set; }

        [JsonPropertyName("app")]
        public Application App { get; set; }
    }
}