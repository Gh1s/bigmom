using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Csb.BigMom.App
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Mvc.
            services.AddMvc();

            // We remap the claim type map to refine them after.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (Environment.IsDevelopment())
            {
                // Never do this in production.
                IdentityModelEventSource.ShowPII = true;
            }

            // The cookie authentication scheme is used to store the authentication ticket built after a successful login.
            // The OpenID connect scheme is used to authenticate users, that's why it's setup as the default challenge scheme.
            services
                .AddAuthentication(config =>
                {
                    config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
                {
                    var authSection = Configuration.GetSection("Authentication");
                    authSection.Bind(config);

                    config.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role
                    };
                    config.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        ctx.ProtocolMessage.SetParameter("audience", authSection.GetValue<string>("Audience"));
                        return Task.CompletedTask;
                    };

                    // We remap the claims to avoid the mess with the default mapping.
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Subject, JwtClaimTypes.Subject, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.PreferredUserName, JwtClaimTypes.PreferredUserName, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Name, JwtClaimTypes.Name, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.GivenName, JwtClaimTypes.GivenName, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.FamilyName, JwtClaimTypes.FamilyName, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Picture, JwtClaimTypes.Picture, ClaimValueTypes.String);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Email, JwtClaimTypes.Email, ClaimValueTypes.Email);
                    config.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.EmailVerified, JwtClaimTypes.EmailVerified, ClaimValueTypes.Boolean);
                });

            // Our default authorization policy requires user to be authenticated.
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            // Token service.
            services.AddHttpContextAccessor();
            services.AddHttpClient(TokenServiceOptions.HttpClient, (provider, client) =>
            {
                client.BaseAddress = new Uri(provider.GetRequiredService<IOptionsMonitor<TokenServiceOptions>>().CurrentValue.Authority);
            });
            services.Configure<TokenServiceOptions>(Configuration.GetSection("Authentication"));
            services.AddScoped<ITokenService, TokenService>();

            // Angular
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";

                var pathBase = Configuration.GetValue<string>("PathBase");
                if (pathBase != null && pathBase != "/")
                {
                    var path = Path.Combine(configuration.RootPath, "index.html");
                    var content = File.ReadAllText(path);
                    var replacedContent = Regex.Replace(content, "<base href.*?/?>", $"<base href=\"{pathBase}{(pathBase.EndsWith("/") ? "" : "/")}\">");
                    File.WriteAllText(path, replacedContent);
                }
            });

            // Healthchecks.
            services.AddHealthChecks();

            services.Configure<ApiOptions>(Configuration.GetSection("Api"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UsePathBase(Configuration.GetValue<string>("PathBase"));

            app.UseHealthChecks("/health");

            app.UseStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
