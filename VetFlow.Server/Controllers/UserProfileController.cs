using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly GraphService _graphService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(GraphService graphService, ILogger<UserProfileController> logger)
        {
            _graphService = graphService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation("User profile request received");
            try
            {
                string? upn;
                if (string.IsNullOrEmpty(upn = User.FindFirst(ClaimTypes.Upn)?.Value))
                    return Unauthorized(new ErrorResponse
                    {
                        Error = "missing_user_claim",
                        ErrorDescription = "User principal name claim not found in token"
                    });

                _logger.LogDebug("Retrieving user profile information for UPN: {Upn}", upn);
                var userEmail =
                            (await _graphService.GetUserNameAndEmailByPrincipalNameAsync(upn)).Email
                             ?? User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                             ?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                             ?? "user@example.com";

                var userName =
                            (await _graphService.GetUserNameAndEmailByPrincipalNameAsync(upn)).DisplayName
                            ?? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                            ?? User.Identity?.Name
                            ?? "User";

                _logger.LogInformation("Successfully retrieved user profile for: {Email}", userEmail);

                return Ok(new UserProfileResponse
                {
                    Name = userName,
                    Email = userEmail,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving user profile");

                return StatusCode(500, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "An unexpected error occurred while retrieving user profile"
                });
            }
        }
    }
}