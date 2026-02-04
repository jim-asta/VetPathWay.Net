using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Security.Cryptography;
using System.Text;
using VetFlow.Server.DTOs;
using VetFlow.Server.Enums;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class SsoRedirectController(IConfiguration config, ILogger<SsoRedirectController> logger) : ControllerBase
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<SsoRedirectController> _logger = logger;

        [HttpGet("{idp}/{email?}")]
        public IActionResult Get(string idp, string? email = null)
        {
            try
            {
                if (!Enum.TryParse<Idp>(idp, ignoreCase: true, out Idp provider))
                    return BadRequest("Invalid identity provider");

                // PKCE
                var (verifier, challenge) = GeneratePkce();
                string? state;
                // Store verifier in session for later token exchange
                HttpContext.Session.SetString("pkce_verifier", verifier);
                HttpContext.Session.SetString("pkce_provider", provider.ToString());
                HttpContext.Session.SetString("oauth_state", state = Guid.NewGuid().ToString());


                string redirectUrl = provider switch            // Prepare redirectUrl for either Google or Microsoft (the only options now)
                {
                    Idp.Google =>
                        QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth",
                        new Dictionary<string, string?>
                        {
                            ["client_id"] = _config["Google:ClientId"], // You'll need Google OAuth credentials
                            ["response_type"] = "code",
                            ["redirect_uri"] = _config["RedirectUri"],
                            ["scope"] = _config["Google:Scopes"],
                            ["code_challenge"] = challenge,
                            ["code_challenge_method"] = "S256",
                            ["state"] = state,
                            ["access_type"] = "offline",
                            ["prompt"] = "consent"
                            //["domain_hint"] = "gmail.com"
                        }.Concat(!string.IsNullOrEmpty(email)
                                ? new Dictionary<string, string?> { ["login_hint"] = email }
                                : [])),

                    Idp.Microsoft =>
                        QueryHelpers.AddQueryString("https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/oauth2/v2.0/authorize",
                        new Dictionary<string, string?>
                        {
                            ["client_id"] = _config["AzureAd:ClientId"],
                            ["response_type"] = "code",
                            ["redirect_uri"] = _config["RedirectUri"],
                            ["response_mode"] = "query",
                            ["scope"] = _config["AzureAd:Scopes"],
                            ["code_challenge"] = challenge,
                            ["code_challenge_method"] = "S256",
                            ["state"] = state, // CSRF protection
                            ["domain_hint"] = "consumers"
                        }.Concat(!string.IsNullOrEmpty(email)
                                ? new Dictionary<string, string?> { ["login_hint"] = email }
                                : [])),
                    _ => throw new ArgumentOutOfRangeException(nameof(idp), idp, "Unsupported identity provider")
                };

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SSO redirect for email: {Email}", email);
                return StatusCode(statusCode: 500, new ErrorResponse
                {
                    Error = "sso_redirect_failed",
                    ErrorDescription = "An error occurred while processing the SSO redirect. Please try again."
                });
            }
        }

        private static (string verifier, string challenge) GeneratePkce()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var verifier = WebEncoders.Base64UrlEncode(bytes);

            var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
            var challenge = WebEncoders.Base64UrlEncode(hash);

            return (verifier, challenge);
        }
    }
}
