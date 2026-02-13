using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using VetFlow.Server.DTOs;
using VetFlow.Server.Enums;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class SsoCallbackController(IConfiguration config, ILogger<SsoCallbackController> logger, IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<SsoCallbackController> _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                //if (!string.IsNullOrEmpty(error))
                //    // Redirect to front-end with error
                //    return Redirect(frontendRedirectUri + "/login?error=" + Uri.EscapeDataString(error));

                var sessionState = HttpContext.Session.GetString("oauth_state");
                if (sessionState != state)
                    return BadRequest(new { error = "invalid_state" });

                var verifier = HttpContext.Session.GetString("pkce_verifier");
                if (string.IsNullOrEmpty(verifier))
                    return BadRequest(new { error = "missing_verifier" });

                if (string.IsNullOrEmpty(HttpContext.Session.GetString("pkce_provider")) || !Enum.TryParse<Idp>(HttpContext.Session.GetString("pkce_provider"), out Idp provider))
                    return BadRequest(new { error = "missing_provider" });

                Token? tokenResponse = await ExchangeCodeForTokens(code, verifier, provider);
                if (tokenResponse == null)
                    return StatusCode(statusCode: StatusCodes.Status500InternalServerError, new ErrorResponse
                    {
                        Error = "token_exchange_failed",
                        ErrorDescription = "Could not exchange code for tokens"
                    });

                // Clear session
                HttpContext.Session.Remove("pkce_verifier");
                HttpContext.Session.Remove("pkce_provider");
                HttpContext.Session.Remove("oauth_state");

                // Redirect back to front-end with success
                return Redirect((_config["FrontendRedirectUri"] ?? "http://localhost:4200") + "/login?token=" + tokenResponse.IdToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SSO callback.");
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "sso_redirect_failed",
                    ErrorDescription = "An error occurred while processing the SSO redirect. Please try again."
                });
            }
        }

        private async Task<Token?> ExchangeCodeForTokens(string code, string verifier, Idp provider)
        {
            var (tokenEndpoint, formData) = provider switch
            {
                Idp.Google => (
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["client_id"] = _config["Google:ClientId"] ?? "",
                        ["client_secret"] = _config["Google:ClientSecret"] ?? "",
                        ["grant_type"] = "authorization_code",
                        ["code"] = code,
                        ["redirect_uri"] = _config["RedirectUri"] ?? "",
                        ["code_verifier"] = verifier
                    }
                )),
                Idp.Microsoft => (
                    "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/oauth2/v2.0/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["client_id"] = _config["AzureAd:ClientId"] ?? "",
                        ["grant_type"] = "authorization_code",
                        ["code"] = code,
                        ["client_secret"] = _config["AzureAd:ClientSecret"],
                        ["redirect_uri"] = _config["RedirectUri"] ?? "",
                        ["code_verifier"] = verifier,
                        ["scope"] = "openid profile email"
                    }
                )),
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };


            var response = await _httpClientFactory.CreateClient().PostAsync(tokenEndpoint, formData);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token exchange error: {errorContent}", await response.Content.ReadAsStringAsync());
                throw new HttpRequestException(message: "Token exchange failed with status " + response.StatusCode + ": " + await response.Content.ReadAsStringAsync(), inner: null, 
                    statusCode: response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<Token>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
