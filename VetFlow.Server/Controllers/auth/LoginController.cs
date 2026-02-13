using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController(IHttpClientFactory factory, IConfiguration config, IGraphService graphService, ILogger<LoginController> logger) : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory = factory;
        private readonly IConfiguration _config = config;
        private readonly IGraphService _graphService = graphService;
        private readonly ILogger<LoginController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            try
            {
                string? userPrincipalName;
                if ((userPrincipalName = await _graphService.GetUserPrincipalNameByEmailAsync(request.Email)) == null)
                {
                    _logger.LogWarning("Login failed - user not found for email: {Email}", request.Email);
                    return BadRequest(new ErrorResponse
                    {
                        Error = "invalid_grant",
                        ErrorDescription = "Invalid email or password"
                    });
                }

                _logger.LogDebug("Authenticating user with Azure AD: {Email}", request.Email);
                var client = _httpFactory.CreateClient();
                var tokenUrl = "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/oauth2/v2.0/token";

                var content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string,string>("grant_type","password"),
                    new KeyValuePair<string,string>("client_id", _config["AzureAd:ClientId"] ?? ""),
                    new KeyValuePair<string,string>("client_secret", _config["AzureAd:ClientSecret"] ?? ""),
                    new KeyValuePair<string,string>("scope", "api://" + (_config["AzureAd:ClientId"] ?? "") + "/access_as_user"),
                    new KeyValuePair<string,string>("username", userPrincipalName),
                    new KeyValuePair<string,string>("password",request.Password)
                ]);

                var response = await client.PostAsync(tokenUrl, content);
                var body = await response.Content.ReadFromJsonAsync<Token>();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Authentication failed for email: {Email}. Error: {Error}", 
                        request.Email, body?.Error ?? "unknown");

                    return BadRequest(new ErrorResponse
                    {
                        Error = body?.Error ?? "authentication_failed",
                        ErrorDescription = body?.ErrorDescription ?? "Authentication failed"
                    });
                }

                _logger.LogInformation("Successful login for email: {Email}", request.Email);
                return Ok(new LoginResponse
                {
                    TokenType = body.TokenType,
                    AccessToken = body.AccessToken,
                    ExpiresIn = body.ExpiresIn,
                    Scope = body.Scope,
                    ExtExpiresIn = body.ExtExpiresIn,
                    IdToken = body.IdToken,
                    RefreshToken = body.RefreshToken
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error during login for email: {Email}", request.Email);
                return StatusCode(statusCode: StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "Failed to communicate with authentication service"
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid response from authentication service for email: {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "Invalid response from authentication service"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "An unexpected error occurred during login"
                });
            }
        }
    }
}
