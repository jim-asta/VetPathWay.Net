using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly GraphService _graphService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IHttpClientFactory factory, IConfiguration config, GraphService graphService, ILogger<LoginController> logger)
        {
            _httpFactory = factory;
            _config = config;
            _graphService = graphService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginRequest req)
        {
            try
            {
                string? userPrincipalName = await _graphService.GetUserPrincipalNameByEmailAsync(req.Email);
                if (userPrincipalName == null)
                {
                    return BadRequest(new
                    {
                        error = "invalid_grant",
                        error_description = "Invalid email or password"
                    });
                }

                var client = _httpFactory.CreateClient();
                var tokenUrl = "https://login.microsoftonline.com/" + _config["AzureAd:TenantId"] + "/oauth2/v2.0/token";

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string,string>("grant_type","password"),
                new KeyValuePair<string,string>("client_id", _config["AzureAd:ClientId"] ?? ""),
                new KeyValuePair<string,string>("client_secret", _config["AzureAd:ClientSecret"] ?? ""),
                new KeyValuePair<string,string>("scope", "api://" + (_config["AzureAd:ClientId"] ?? "") + "/access_as_user"),
                new KeyValuePair<string,string>("username", userPrincipalName),
                new KeyValuePair<string,string>("password",req.Password)
            });

                var response = await client.PostAsync(tokenUrl, content);
                var body = await response.Content.ReadFromJsonAsync<Token>();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Authentication failed for email: " + req.Email + " Error: " + body?.error);

                    return BadRequest(new
                    {
                        error = body?.error ?? "authentication_failed",
                        error_description = body?.error_description ?? "Authentication failed"
                    });
                }

                _logger.LogInformation("Successful login for email: {Email}", req.Email);
                return Ok(body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", req.Email);
                return StatusCode(500, new
                {
                    error = "server_error",
                    error_description = "An unexpected error occurred during login"
                });
            }
        }
    }
}
