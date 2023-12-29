using MediatR;
using System.Text.Json.Serialization;

namespace Csb.BigMom.Infrastructure.Data
{
    public class CommerceIndexRequest : IRequest
    {
        [JsonPropertyName("commerce_id")]
        public int CommerceId { get; set; }
    }
}