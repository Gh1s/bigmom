using MediatR;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Data
{
    public class TlcIndexRequest : IRequest
    {
        [JsonPropertyName("tlc_id")]
        public int TlcId { get; set; }
    }
}
