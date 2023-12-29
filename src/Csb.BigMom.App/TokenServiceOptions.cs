namespace Csb.BigMom.App
{
    /// <summary>
    /// Configuration options for <see cref="TokenService"/>.
    /// </summary>
    public class TokenServiceOptions
    {
        public const string HttpClient = "token";
        public const string TokenEndpoint = "oauth2/token";

        /// <summary>
        /// The IDP.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// The client_id to interact with the IDP.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The client_secret to interact with the IDP.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The threshold in seconds to add to the current UTC date time, that can state if an acces_token has expired.
        /// </summary>
        public double TokenExpiryThresholdSeconds { get; set; }
    }
}
