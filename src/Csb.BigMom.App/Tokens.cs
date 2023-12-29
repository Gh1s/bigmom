using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.App
{
    /// <summary>
    /// Models id_token, access_token & its expiration.
    /// </summary>
    public class Tokens
    {
        /// <summary>
        /// The access_token exipiration date.
        /// </summary>
        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// The access_token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The id_token.
        /// </summary>
        [JsonPropertyName("id_token")]
        public string IdentityToken { get; set; }
    }
}
