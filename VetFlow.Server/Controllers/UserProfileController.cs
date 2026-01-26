using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VetFlow.Server.Controllers.auth;
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
        private readonly ILogger<LoginController> _logger;

        public UserProfileController(GraphService graphService, ILogger<LoginController> logger)
        {
            _graphService = graphService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string? upn;
            if (string.IsNullOrEmpty(upn = User.FindFirst(ClaimTypes.Upn)?.Value))
                return Unauthorized(new
                {
                    error = "missing_user_claim",
                    error_description = "User principal name claim not found in token"
                });

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


            return Ok(new UserProfile
            {
                Name = userName,
                Email = userEmail,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            });
        }
    }
}