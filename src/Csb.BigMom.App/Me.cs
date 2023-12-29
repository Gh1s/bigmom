using System;
using System.Text.Json.Serialization;

namespace Csb.BigMom.App
{
    public class Me
    {
        public User User { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTimeOffset ExpiresAt { get; set; }

        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; }

        [JsonPropertyName("api")]
        public ApiOptions Api { get; set; }
    }

    public class User
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
    }
}
