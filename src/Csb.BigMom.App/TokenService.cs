using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace Csb.BigMom.App
{
    /// <summary>
    /// Provides an implementation of <see cref="ITokenService"/>.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly ISystemClock _systemClock;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationService _authService;
        private readonly IOptionsMonitor<TokenServiceOptions> _optionsMonitor;

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        private TokenServiceOptions Options => _optionsMonitor.CurrentValue;

        public TokenService(
            ISystemClock systemClock,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IAuthenticationService authService,
            IOptionsMonitor<TokenServiceOptions> optionsMonitor)
        {
            _systemClock = systemClock;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _authService = authService;
            _optionsMonitor = optionsMonitor;
        }

        /// <inheritdoc/>
        public async Task<Tokens> GetTokensAsync(CancellationToken cancellationToken)
        {
            var tokens = new Tokens
            {
                ExpiresAt = DateTimeOffset.Parse(await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "expires_at"))
            };

            if (tokens.ExpiresAt > _systemClock.UtcNow.AddSeconds(Options.TokenExpiryThresholdSeconds))
            {
                tokens.AccessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, TokenResponse.AccessToken);
                tokens.IdentityToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, TokenResponse.IdentityToken);
            }
            else
            {
                await RefreshTokensAsync(tokens, cancellationToken);
            }

            return tokens;
        }

        private async Task RefreshTokensAsync(Tokens tokens, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(TokenServiceOptions.HttpClient);

            // Refreshing the token with the IDP.
            using var refreshRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { TokenRequest.GrantType, GrantTypes.RefreshToken },
                { TokenRequest.RefreshToken, await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, TokenRequest.RefreshToken) },
                { TokenRequest.ClientId, Options.ClientId },
                { TokenRequest.ClientSecret, Options.ClientSecret }
            });
            using var refreshResponse = await client.PostAsync(TokenServiceOptions.TokenEndpoint, refreshRequest, cancellationToken);
            using var refreshResponseStream = await refreshResponse.Content.ReadAsStreamAsync(CancellationToken.None);
            var document = await JsonDocument.ParseAsync(refreshResponseStream, cancellationToken: CancellationToken.None);

            tokens.ExpiresAt = _systemClock.UtcNow.AddSeconds(document.RootElement.GetProperty(TokenResponse.ExpiresIn).GetInt32());
            tokens.AccessToken = document.RootElement.GetProperty(TokenResponse.AccessToken).GetString();
            tokens.IdentityToken = document.RootElement.GetProperty(TokenResponse.IdentityToken).GetString();
            var refreshToken = document.RootElement.GetProperty(TokenResponse.RefreshToken).GetString();

            // To refresh the authenticaton properties, we must update the authentication ticket.
            var authTicket = await _authService.AuthenticateAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.UpdateTokenValue("expires_at", JsonSerializer.Serialize(tokens.ExpiresAt).Replace("\"", ""));
            authTicket.Properties.UpdateTokenValue(TokenResponse.AccessToken, tokens.AccessToken);
            authTicket.Properties.UpdateTokenValue(TokenResponse.IdentityToken, tokens.IdentityToken);
            authTicket.Properties.UpdateTokenValue(TokenResponse.RefreshToken, refreshToken);
            await _authService.SignInAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, authTicket.Principal, authTicket.Properties);
        }
    }
}
