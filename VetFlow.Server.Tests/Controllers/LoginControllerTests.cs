using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VetFlow.Server.Controllers.auth;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Tests.Controllers.Auth
{
    public class LoginControllerTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpFactory;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IGraphService> _mockGraphService;
        private readonly Mock<ILogger<LoginController>> _mockLogger;
        private readonly LoginController _controller;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public LoginControllerTests()
        {
            _mockHttpFactory = new Mock<IHttpClientFactory>();
            _mockConfig = new Mock<IConfiguration>();
            _mockGraphService = new Mock<IGraphService>();
            _mockLogger = new Mock<ILogger<LoginController>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Setup configuration
            _mockConfig.Setup(c => c["AzureAd:TenantId"]).Returns("test-tenant-id");
            _mockConfig.Setup(c => c["AzureAd:ClientId"]).Returns("test-client-id");
            _mockConfig.Setup(c => c["AzureAd:ClientSecret"]).Returns("test-client-secret");

            // Setup HttpClient with mocked handler
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _controller = new LoginController(
                _mockHttpFactory.Object,
                _mockConfig.Object,
                _mockGraphService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Post_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest { Email = "nonexistent@test.com", Password = "password123" };
            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("invalid_grant", errorResponse.Error);
            Assert.Equal("Invalid email or password", errorResponse.ErrorDescription);
        }

        [Fact]
        public async Task Post_SuccessfulLogin_ReturnsOkWithToken()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var tokenResponse = new Token
            {
                TokenType = "Bearer",
                AccessToken = "test-access-token",
                ExpiresIn = 3600,
                Scope = "api://test-client-id/access_as_user",
                ExtExpiresIn = 3600,
                IdToken = "test-id-token",
                RefreshToken = "test-refresh-token"
            };

            var responseContent = JsonSerializer.Serialize(tokenResponse);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("Bearer", loginResponse.TokenType);
            Assert.Equal("test-access-token", loginResponse.AccessToken);
            Assert.Equal(3600, loginResponse.ExpiresIn);
            Assert.Equal("test-refresh-token", loginResponse.RefreshToken);
        }

        [Fact]
        public async Task Post_AzureAdAuthenticationFails_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "wrongpassword" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var errorResponse = new Token
            {
                Error = "invalid_grant",
                ErrorDescription = "Invalid username or password"
            };

            var responseContent = JsonSerializer.Serialize(errorResponse);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("invalid_grant", error.Error);
            Assert.Equal("Invalid username or password", error.ErrorDescription);
        }

        [Fact]
        public async Task Post_AzureAdReturnsNullBody_ReturnsServerError()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Equal("server_error", error.Error);
            Assert.Equal("Invalid response from authentication service", error.ErrorDescription);
        }

        [Fact]
        public async Task Post_HttpRequestException_ReturnsServerError()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Equal("server_error", error.Error);
            Assert.Equal("Failed to communicate with authentication service", error.ErrorDescription);
        }

        [Fact]
        public async Task Post_UnexpectedException_ReturnsServerError()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act
            var result = await _controller.Post(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusCodeResult.Value);
            Assert.Equal("server_error", error.Error);
            Assert.Equal("An unexpected error occurred during login", error.ErrorDescription);
        }

        [Fact]
        public async Task Post_ValidRequest_SendsCorrectTokenRequest()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var tokenResponse = new Token
            {
                TokenType = "Bearer",
                AccessToken = "test-access-token",
                ExpiresIn = 3600
            };

            HttpRequestMessage? capturedRequest = null;
            var responseContent = JsonSerializer.Serialize(tokenResponse);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(httpResponse);

            // Act
            await _controller.Post(request);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest.Method);
            Assert.Contains("test-tenant-id", capturedRequest.RequestUri?.ToString());
            Assert.Contains("/oauth2/v2.0/token", capturedRequest.RequestUri?.ToString());

            var formContent = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Contains("grant_type=password", formContent);
            Assert.Contains("client_id=test-client-id", formContent);
            Assert.Contains("username=user%40test.onmicrosoft.com", formContent);
            Assert.Contains("password=password123", formContent);
        }

        [Fact]
        public async Task Post_LogsInformationOnLoginAttempt()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login attempt for email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_LogsWarningOnUserNotFound()
        {
            // Arrange
            var request = new LoginRequest { Email = "nonexistent@test.com", Password = "password123" };
            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync((string?)null);

            // Act
            await _controller.Post(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login failed - user not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Post_LogsSuccessfulLogin()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "password123" };
            var userPrincipalName = "user@test.onmicrosoft.com";

            _mockGraphService
                .Setup(g => g.GetUserPrincipalNameByEmailAsync(request.Email))
                .ReturnsAsync(userPrincipalName);

            var tokenResponse = new Token
            {
                TokenType = "Bearer",
                AccessToken = "test-access-token",
                ExpiresIn = 3600
            };

            var responseContent = JsonSerializer.Serialize(tokenResponse);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            // Act
            await _controller.Post(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successful login")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // End-to-End Integration Tests
    public class LoginControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LoginControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

                    // Remove real IHttpClientFactory
                    var httpClientFactoryDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IHttpClientFactory));
                    if (httpClientFactoryDescriptor != null)
                        services.Remove(httpClientFactoryDescriptor);

                    // Add test doubles
                    services.AddSingleton<IGraphService, InMemoryGraphService>();
                    services.AddSingleton<IHttpClientFactory, InMemoryHttpClientFactory>();
                });

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AzureAd:TenantId"] = "test-tenant",
                        ["AzureAd:ClientId"] = "test-client",
                        ["AzureAd:ClientSecret"] = "test-secret"
                    });
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Post_ValidCredentials_Returns200WithAccessToken()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/login", new LoginRequest
                {
                    Email = "valid@test.com",
                    Password = "ValidPassword123!"
                });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse);
            
            Assert.Equal("Bearer", loginResponse.TokenType);
                        
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
            
            Assert.True(loginResponse.ExpiresIn > 0);
            
            Assert.NotNull(loginResponse.RefreshToken);
            Assert.NotEmpty(loginResponse.RefreshToken);
        }

        [Fact]
        public async Task Post_InvalidEmailFormat_Returns400ValidationError()
        {
            // Just test validation
            var response = await _client.PostAsJsonAsync("/api/login", new LoginRequest
                {
                    Email = "not-valid-email",
                    Password = "pass"
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        // In-Memory Test Doubles
        public class InMemoryHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name)
            {
                var handler = new InMemoryAzureAdHandler();
                return new HttpClient(handler);
            }
        }

        public class InMemoryAzureAdHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                // Simulate successful Azure AD token response
                var tokenResponse = new
                {
                    token_type = "Bearer",
                    access_token = "test-access-token-abc123",
                    expires_in = 3600,
                    ext_expires_in = 3600,
                    scope = "api://test-client/access_as_user",
                    id_token = "test-id-token-xyz789",
                    refresh_token = "test-refresh-token-def456"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(tokenResponse),
                    System.Text.Encoding.UTF8,
                    "application/json");

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content
                });
            }
        }
    }
}