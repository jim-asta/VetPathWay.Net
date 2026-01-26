using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateAccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<CreateAccountController> _logger;
        private readonly GraphService _graphService;

        public CreateAccountController(GraphService graphService, IConfiguration config, ILogger<CreateAccountController> logger)
        {
            _graphService = graphService;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateAccountRequest req)
        {
            try
            {
                // Check if user already exists
                if (!await _graphService.UserExistsAsync(req.Email))
                {
                    // Create the new user
                    Microsoft.Graph.Models.User? createdUser;
                    if ((createdUser = await _graphService.CreateUserAsync(req.Email, req.Password)) == null)
                    {
                        _logger.LogError("Failed to create user for email: " + req.Email);
                        return StatusCode(500, new CreateAccountResponse
                        {
                            Success = false,
                            Message = "Failed to create user for email: " + req.Email
                        });
                    }

                    _logger.LogInformation("Successfully created user account: {Email}", req.Email);

                    return Ok(new CreateAccountResponse
                    {
                        Success = true,
                        Message = "Account created successfully",
                        UserId = createdUser?.Id
                    });
                }
                else                {
                    _logger.LogWarning("Attempt to create account with existing email: {Email}", req.Email);
                    return Conflict(new CreateAccountResponse  // Returns 409
                    {
                        Success = false,
                        Message = "An account with this Email already exists"
                    });
                }

            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Microsoft Graph API error while creating user: {Email}", req.Email);

                return BadRequest(new CreateAccountResponse
                {
                    Success = false,
                    Message = "Failed to create account: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating user account: {Email}", req.Email);

                return StatusCode(500, new CreateAccountResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while creating your account"
                });
            }
        }

        public class PasswordStrengthAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value is not string Password || string.IsNullOrWhiteSpace(Password))
                    return ValidationResult.Success; // Let [Required] handle empty values

                int categoriesFound = 0;

                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]")) categoriesFound++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]")) categoriesFound++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[0-9]")) categoriesFound++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[@#$%^&*\-_!+=[\]{}|\\:',.\?\/`~""();<> ]")) categoriesFound++;

                if (categoriesFound < 3)
                    return new ValidationResult(ErrorMessage ?? "Password not strong enough");

                return ValidationResult.Success;
            }
        }
    }
}