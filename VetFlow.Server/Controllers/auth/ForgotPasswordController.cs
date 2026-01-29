using MailerSend.AspNetCore;
using MailerSend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.ODataErrors;
using System.Net;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;
using ReverseMarkdown;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController(GraphService graphService, ILogger<ForgotPasswordController> logger, MailerSendService mailerSend) : ControllerBase
    {
        private readonly GraphService _graphService = graphService;
        private readonly ILogger<ForgotPasswordController> _logger = logger;
        private readonly MailerSendService _mailerSend = mailerSend;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("Password reset request received for user with email: {Email}", request.Email);
            try
            {
                var recipients = new List<Recipient>
{
    new Recipient
    {
        Email = "your-test-email@gmail.com",  // Use your own email for testing
        Name = "Test User"
    }
};

                await _mailerSend.SendMailAsync(
                    to: recipients,
                    subject: "Test Email",
                    html: "<p>Test message</p>",
                    text: "Test message");

                _logger.LogDebug("Attempting to reset password for user with email: {Email}", request.Email);
                if (await _graphService.GetUserPrincipalNameByEmailAsync(request.Email) == null)
                {
                    _logger.LogError("Password reset failed - User for email: {Email} does not exist.", request.Email);
                    // SECURITY: Don't reveal if user exists or not - always return success
                    // This prevents email enumeration attacks
                    return Ok(new ForgotPasswordResponse
                    {
                        Success = true,
                        Message = "If an account exists with that email, a password reset link has been sent."
                    });
                }

                string? htmlContent;
                await _mailerSend.SendMailAsync(to: new List<Recipient>
                                                    {
                                                        new Recipient
                                                        {
                                                            Email = request.Email,
                                                            Name = request.Email.Split('@')[0] // Use part before @ as name
                                                        }
                                                    },
                                                    subject: "Password Reset Instructions",
                                                    html: htmlContent = @"
                                                        <p>Hello,</p>
                                                        <p>To reset your password, please follow these steps:</p>
                                                        <ol>
                                                            <li>Click this link: <a href='https://passwordreset.microsoftonline.com/?username=" + Uri.EscapeDataString(request.Email) + @"'>Reset Password</a></li>
                                                            <li>Enter your email address: <strong>{request.Email}</strong></li>
                                                            <li>Verify your identity using one of your registered methods</li>
                                                            <li>Create a new password</li>
                                                        </ol>
                                                        <p>If you didn't request this, please ignore this email.</p>",
                                                    text: new Converter().Convert(htmlContent));     // Exception-based error handling, so no need to check null here

                _logger.LogInformation("Password reset successful for user with: {Email}", request.Email);
                return Ok(new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "Password reset successful."
                });
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
            {
                // 401 - Invalid API token
                _logger.LogError(httpEx, "MailerSend authentication failed. Check your API token configuration.");
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Email service is temporarily unavailable. Please try again later."
                });
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.Forbidden)
            {
                // 403 - Domain not verified or insufficient permissions
                _logger.LogError(httpEx, "MailerSend domain not verified or insufficient permissions.");
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Email service configuration error. Please contact support."
                });
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == (HttpStatusCode)422)
            {
                // 422 - Validation error (invalid email format, etc.)
                _logger.LogError(httpEx, "MailerSend validation error for email: {Email}", request.Email);
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Failed to send password reset email. Please try again later."
                });
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == (HttpStatusCode)429)
            {
                // 429 - Rate limit exceeded
                _logger.LogError(httpEx, "MailerSend rate limit exceeded.");
                return StatusCode(429, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Too many requests. Please try again in a few minutes."
                });
            }
            catch (HttpRequestException httpEx)
            {
                // Other HTTP errors
                _logger.LogError(httpEx, "MailerSend HTTP error. Status: {StatusCode}", httpEx.StatusCode);
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Failed to send password reset email. Please try again later."
                });
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                _logger.LogError(ex, "MailerSend request timed out for user: {Email}", request.Email);
                return StatusCode(504, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Request timed out. Please try again later."
                });
            }
            catch (ODataError ex)
            {
                _logger.LogError(ex, "Microsoft Graph API error while resetting password for user with: {Email}. Error code: {ErrorCode}", 
                    request.Email, ex.Error?.Code);

                return StatusCode(statusCode: 500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Failed to reset password: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for user with email: {Email}", request.Email);

                return StatusCode(statusCode: 500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while resetting password"
                });
            }
        }
    }
}
