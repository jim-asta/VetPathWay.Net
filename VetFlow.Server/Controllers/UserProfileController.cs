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
        private readonly IGraphService _graphService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IGraphService graphService, ILogger<UserProfileController> logger)
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
                var userEmail =
                             User.FindFirst(ClaimTypes.Email)?.Value
                             ?? User.FindFirst("preferred_username")?.Value
                             ?? User.FindFirst("email")?.Value
                             ?? User.FindFirst("emails")?.Value // Some providers use "emails"
                             ?? "user@example.com";

                var userName =
                            User.FindFirst("name")?.Value
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

                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "server_error",
                    ErrorDescription = "An unexpected error occurred while retrieving user profile"
                });
            }
        }
    }
}