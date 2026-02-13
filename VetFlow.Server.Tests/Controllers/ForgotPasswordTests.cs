using MailerSend.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.ODataErrors;
using Moq;
using System.Net;
using System.Net.Http.Json;
using VetFlow.Server.Controllers.auth;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;
using Xunit;

namespace VetFlow.Server.Tests.Controllers.Auth
{
    public class ForgotPasswordControllerTests
    {
        private readonly Mock<IGraphService> _mockGraphService;
        private readonly Mock<ILogger<ForgotPasswordController>> _mockLogger;
        private readonly Mock<IMailerSendService> _mockMailerSend;
        private readonly ForgotPasswordController _controller;

        public ForgotPasswordControllerTests()
        {
            _mockGraphService = new Mock<IGraphService>();
            _mockLogger = new Mock<ILogger<ForgotPasswordController>>();
            _mockMailerSend = new Mock<IMailerSendService>();

            _controller = new ForgotPasswordController(
                _mockGraphService.Object,
                _mockLogger.Object,
                _mockMailerSend.Object
            );
        }

        [Fact]
        public async Task Post_UserNotFound_ReturnsOkWithGenericMessage()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "nonexistent@test.com" };
            bool emailWasSent = false;
            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync((string?)null);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Callback(() => emailWasSent = true)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ForgotPasswordResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("If an account exists with that email, a password reset link has been sent.", response.Message);

            // Verify email was NOT sent
            Assert.False(emailWasSent, "Email should not have been sent for non-existent user");
        }

        [Fact]
        public async Task Post_SuccessfulPasswordReset_ReturnsOkAndSendsEmail()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";
            bool emailWasSent = false;
            IEnumerable<Recipient>? sentRecipients = null;
            string? sentSubject = null;
            string? sentHtml = null;

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipient>, IEnumerable<Recipient>?, IEnumerable<Recipient>?, string?, string?, string?, string?, IEnumerable<Attachment>?, DateTime?, CancellationToken>(
                    (to, cc, bcc, subj, text, htm, templateId, attachments, sendAt, ct) =>
                    {
                        emailWasSent = true;
                        sentRecipients = to;
                        sentSubject = subj;
                        sentHtml = htm;
                    })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ForgotPasswordResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Password reset successful.", response.Message);

            // Verify email details
            Assert.True(emailWasSent);

            Assert.NotNull(sentRecipients);
            Assert.Single(sentRecipients);

            Assert.Equal(request.Email, sentRecipients.First().Email);

            Assert.Equal("Password Reset Instructions", sentSubject);

            Assert.Contains(Uri.EscapeDataString(userPrincipalName), sentHtml);
        }

        [Fact]
        public async Task Post_SuccessfulPasswordReset_EmailContainsCorrectResetLink()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";
            string? capturedHtml = null;

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipient>, IEnumerable<Recipient>?, IEnumerable<Recipient>?, string?, string?, string?, string?, IEnumerable<Attachment>?, DateTime?, CancellationToken>(
                    (to, cc, bcc, subj, text, htm, templateId, attachments, sendAt, ct) =>
                    {
                        capturedHtml = htm;
                    })
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(request);

            // Assert
            Assert.NotNull(capturedHtml);
            Assert.Contains("https://passwordreset.microsoftonline.com/", capturedHtml);
            Assert.Contains(Uri.EscapeDataString(userPrincipalName), capturedHtml);
            Assert.Contains("Reset Password", capturedHtml);
        }

        [Fact]
        public async Task Post_MailerSendUnauthorized_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpException = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Email service is temporarily unavailable. Please try again later.", response.Message);
        }

        [Fact]
        public async Task Post_MailerSendForbidden_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpException = new HttpRequestException("Forbidden", null, HttpStatusCode.Forbidden);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Email service configuration error. Please contact support.", response.Message);
        }

        [Fact]
        public async Task Post_MailerSendValidationError_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpException = new HttpRequestException("Validation Error", null, HttpStatusCode.UnprocessableContent);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Failed to send password reset email. Please try again later.", response.Message);
        }

        [Fact]
        public async Task Post_MailerSendRateLimitExceeded_Returns429()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpException = new HttpRequestException("Rate Limit", null, HttpStatusCode.TooManyRequests);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status429TooManyRequests, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Too many requests. Please try again in a few minutes.", response.Message);
        }

        [Fact]
        public async Task Post_MailerSendOtherHttpError_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpException = new HttpRequestException("Server Error", null, HttpStatusCode.InternalServerError);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Failed to send password reset email. Please try again later.", response.Message);
        }

        [Fact]
        public async Task Post_MailerSendTimeout_Returns504()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TaskCanceledException("Timeout"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status504GatewayTimeout, statusCodeResult.StatusCode);
            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Request timed out. Please try again later.", response.Message);
        }

        [Fact]
        public async Task Post_GraphApiODataError_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ThrowsAsync(new ODataError
                {
                    Error = new MainError
                    {
                        Code = "ResourceNotFound",
                        Message = "User not found"
                    }
                });

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Contains("Failed to reset password:", response.Message);
        }

        [Fact]
        public async Task Post_UnexpectedException_ReturnsServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

            var response = Assert.IsType<ForgotPasswordResponse>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("An unexpected error occurred while resetting password", response.Message);
        }

        [Fact]
        public async Task Post_LogsInformationOnRequest()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync((string?)null);

            // Act
            await _controller.Post(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password reset request received")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_LogsErrorWhenUserNotFound()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "nonexistent@test.com" };
            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync((string?)null);

            // Act
            await _controller.Post(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password reset failed - User for email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_LogsSuccessWhenEmailSent()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "user@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password reset successful")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_RecipientNameIsEmailPrefix()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "john.doe@test.com" };
            var userPrincipalName = "user@test.onmicrosoft.com";
            Recipient? capturedRecipient = null;

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            // Track if SendMailAsync is called
            _mockMailerSend
                .Setup(m => m.SendMailAsync(    // Need all parameters because Moq has trouble matching optional params otherwise
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<IEnumerable<Recipient>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<Attachment>>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<Recipient>, IEnumerable<Recipient>?, IEnumerable<Recipient>?, string?, string?, string?, string?, IEnumerable<Attachment>?, DateTime?, CancellationToken>(
                    (to, cc, bcc, subj, text, htm, templateId, attachments, sendAt, ct) =>
                    capturedRecipient = to.FirstOrDefault())
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(request);

            // Assert
            Assert.NotNull(capturedRecipient);
            Assert.Equal("john.doe@test.com", capturedRecipient.Email);
            Assert.Equal("john.doe", capturedRecipient.Name);
        }
    }

    // End-to-end integration tests
    public class ForgotPasswordIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ForgotPasswordIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove real IGraphService
                    var graphServiceDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IGraphService));
                    if (graphServiceDescriptor != null)
                        services.Remove(graphServiceDescriptor);

                    // Remove real MailerSendService wrapper
                    var mailerSendWrapperDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMailerSendService));
                    if (mailerSendWrapperDescriptor != null)
                        services.Remove(mailerSendWrapperDescriptor);

                    // Remove real MailerSendService
                    var mailerSendDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(MailerSendService));
                    if (mailerSendDescriptor != null)
                        services.Remove(mailerSendDescriptor);

                    // Add test doubles
                    services.AddSingleton<IGraphService, InMemoryGraphService>();
                    services.AddSingleton<IMailerSendService, InMemoryMailerSendService>();
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Post_ValidRequest_Returns200WithSuccessMessage()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "valid@test.com" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/forgotpassword", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Contains("Password reset successful.", result.Message);
        }

        [Fact]
        public async Task Post_InvalidEmail_Returns400()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/forgotpassword", new ForgotPasswordRequest { Email = "not-an-email" });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        public class InMemoryMailerSendService : IMailerSendService
        {
            public List<EmailRecord> SentEmails { get; } = new();

            public Task SendMailAsync(
                IEnumerable<Recipient> to,
                IEnumerable<Recipient>? cc = null,
                IEnumerable<Recipient>? bcc = null,
                string? subject = null,
                string? text = null,
                string? html = null,
                string? templateId = null,
                IEnumerable<Attachment>? attachments = null,
                DateTime? sendAt = null,
                CancellationToken cancellationToken = default)
            {
                SentEmails.Add(new EmailRecord(
                    To: to.ToList(),
                    Cc: cc?.ToList(),
                    Bcc: bcc?.ToList(),
                    Subject: subject,
                    Text: text,
                    Html: html,
                    TemplateId: templateId
                ));

                return Task.CompletedTask;
            }

            public record EmailRecord(
                List<Recipient> To,
                List<Recipient>? Cc,
                List<Recipient>? Bcc,
                string? Subject,
                string? Text,
                string? Html,
                string? TemplateId
            );
        }
    }
}