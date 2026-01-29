using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models.ODataErrors;
using SendGrid;
using SendGrid.Helpers.Mail;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Controllers.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController(GraphService graphService, ILogger<ForgotPasswordController> logger, IConfiguration config) : ControllerBase
    {
        private readonly GraphService _graphService = graphService;
        private readonly ILogger<ForgotPasswordController> _logger = logger;
        private readonly IConfiguration _config = config;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("Password reset request received for user with email: {Email}", request.Email);
            try
            {
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


                var resetUrl = $"https://passwordreset.microsoftonline.com/?username={Uri.EscapeDataString(request.Email)}";

                var htmlContent = @"
                                        <p>Hello,</p>
                                        <p>To reset your password, please follow these steps:</p>
                                        <ol>
                                            <li>Click this link: <a href='https://passwordreset.microsoftonline.com/?username=" + Uri.EscapeDataString(request.Email) + @"'>Reset Password</a></li>
                                            <li>Enter your email address: <strong>" + request.Email + @"</strong></li>
                                            <li>Verify your identity using one of your registered methods</li>
                                            <li>Create a new password</li>
                                        </ol>";

                var msg = MailHelper.CreateSingleEmail(new EmailAddress("notifications@vetpathway.net", "VetPathway"), 
                    to: new EmailAddress(request.Email), subject: "Password Reset Instructions", ConvertHtmlToPlainText(htmlContent), htmlContent);

                Response? sendEmailResponse;
                if(!(sendEmailResponse = await new SendGridClient(_config["SendGrid:ApiKey"]).SendEmailAsync(msg)).IsSuccessStatusCode)
                {
                    _logger.LogError("SendGrid failed to send email. Status: {StatusCode}, Body: {Body}",
                        sendEmailResponse.StatusCode, await sendEmailResponse.Body.ReadAsStringAsync());
                    return StatusCode(500, new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Failed to send password reset email"
                    });
                }

                _logger.LogInformation("Password reset successful for user with: {Email}", request.Email);
                return Ok(new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "Password reset successful."
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

        private string ConvertHtmlToPlainText(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return string.Empty;

            // Remove HTML tags
            var text = System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);

            // Decode HTML entities (like &nbsp;, &lt;, etc.)
            text = System.Net.WebUtility.HtmlDecode(text);

            // Replace multiple whitespace/newlines with single space
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Trim the result
            return text.Trim();
        }
    }
}
