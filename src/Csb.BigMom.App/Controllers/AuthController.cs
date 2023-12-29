using IdentityModel;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Csb.BigMom.App.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAntiforgery _antiforgery;
        private readonly ApiOptions _options;

        public AuthController(
            ITokenService tokenService,
            IAntiforgery antiforgery,
            IOptionsSnapshot<ApiOptions> optionsSnapshot)
        {
            _tokenService = tokenService;
            _antiforgery = antiforgery;
            _options = optionsSnapshot.Value;
        }

        [Authorize]
        [HttpPost("/logout")]
        [ValidateAntiForgeryToken]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [Route("/frontchannel-logout")]
        public async Task<IActionResult> FrontchannelLogout(string sid)
        {
            if (sid == User.FindFirstValue(JwtClaimTypes.SessionId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok();
            }

            return BadRequest();
        }

        [Authorize]
        [HttpGet("/me")]
        public async Task<Me> Me(CancellationToken cancellationToken)
        {
            var tokens = await _tokenService.GetTokensAsync(cancellationToken);
            return new Me
            {
                User = new User
                {
                    Id = User.FindFirstValue(JwtClaimTypes.Subject),
                    Username = User.FindFirstValue(JwtClaimTypes.PreferredUserName),
                    FirstName = User.FindFirstValue(JwtClaimTypes.GivenName),
                    LastName = User.FindFirstValue(JwtClaimTypes.FamilyName),
                    Email = User.FindFirstValue(JwtClaimTypes.Email)
                },
                AccessToken = tokens.AccessToken,
                ExpiresAt = tokens.ExpiresAt,
                CsrfToken = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken,
                Api = _options
            };
        }
    }
}
