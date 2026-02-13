using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VetFlow.Server.Controllers.auth;
using VetFlow.Server.DTOs;
using VetFlow.Server.Services;

namespace VetFlow.Server.Tests.Controllers.Auth
{
    public class CreateAccountControllerTests
    {
        private readonly Mock<IGraphService> _mockGraphService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<CreateAccountController>> _mockLogger;
        private readonly CreateAccountController _controller;

        public CreateAccountControllerTests()
        {
            _mockGraphService = new Mock<IGraphService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<CreateAccountController>>();
            _controller = new CreateAccountController(
                _mockGraphService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Post_NewUser_ReturnsOkWithSuccessResponse()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "newuser@test.com",
                Password = "SecurePass123!"
            };

            var expectedUser = new User
            {
                Id = "test-user-id-123",
                UserPrincipalName = "newuser@test.onmicrosoft.com",
                Mail = "newuser@test.com"
            };

            _mockGraphService
                .Setup(x => x.UserExistsAsync(request.Email))
                .ReturnsAsync(false);

            _mockGraphService
                .Setup(x => x.CreateUserAsync(request.Email, request.Password))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.Post(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreateAccountResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Account created successfully", response.Message);
            Assert.Equal(expectedUser.Id, response.UserId);

            _mockGraphService.Verify(x => x.UserExistsAsync(request.Email), Times.Once);
            _mockGraphService.Verify(x => x.CreateUserAsync(request.Email, request.Password), Times.Once);
        }

        //[Fact]
        //public async Task Post_ExistingUser_ReturnsConflictResponse()
        //{
        //    // Arrange
        //    var request = new CreateAccountRequest
        //    {
        //        Email = "existing@test.com",
        //        Password = "SecurePass123!"
        //    };

        //    _mockGraphService
        //        .Setup(x => x.UserExistsAsync(request.Email))
        //        .ReturnsAsync(true);

        //    // Act
        //    var result = await _controller.Post(request);

        //    // Assert
        //    var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        //    var response = Assert.IsType<CreateAccountResponse>(conflictResult.Value);
        //    Assert.False(response.Success);
        //    Assert.Equal("An account with this Email already exists", response.Message);

        //    _mockGraphService.Verify(x => x.UserExistsAsync(request.Email), Times.Once);
        //    _mockGraphService.Verify(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        //}

        //[Fact]
        //public async Task Post_GraphServiceReturnsNull_ReturnsInternalServerError()
        //{
        //    // Arrange
        //    var request = new CreateAccountRequest
        //    {
        //        Email = "newuser@test.com",
        //        Password = "SecurePass123!"
        //    };

        //    _mockGraphService
        //        .Setup(x => x.UserExistsAsync(request.Email))
        //        .ReturnsAsync(false);

        //    _mockGraphService
        //        .Setup(x => x.CreateUserAsync(request.Email, request.Password))
        //        .ReturnsAsync((User?)null);

        //    // Act
        //    var result = await _controller.Post(request);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);
        //    var response = Assert.IsType<CreateAccountResponse>(statusCodeResult.Value);
        //    Assert.False(response.Success);
        //    Assert.Contains("Failed to create user for email:", response.Message);
        //}

        //[Fact]
        //public async Task Post_ODataError_ReturnsBadRequest()
        //{
        //    // Arrange
        //    var request = new CreateAccountRequest
        //    {
        //        Email = "invalid@test.com",
        //        Password = "SecurePass123!"
        //    };

        //    var odataError = new ODataError
        //    {
        //        Error = new MainError
        //        {
        //            Message = "Invalid user data"
        //        }
        //    };

        //    _mockGraphService
        //        .Setup(x => x.UserExistsAsync(request.Email))
        //        .ReturnsAsync(false);

        //    _mockGraphService
        //        .Setup(x => x.CreateUserAsync(request.Email, request.Password))
        //        .ThrowsAsync(new Microsoft.Graph.Models.ODataErrors.ODataError
        //        {
        //            Error = new MainError { Message = "Invalid user data" }
        //        });

        //    // Act
        //    var result = await _controller.Post(request);

        //    // Assert
        //    var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        //    var response = Assert.IsType<CreateAccountResponse>(badRequestResult.Value);
        //    Assert.False(response.Success);
        //    Assert.Contains("Failed to create account:", response.Message);
        //}

        //[Fact]
        //public async Task Post_UnexpectedException_ReturnsInternalServerError()
        //{
        //    // Arrange
        //    var request = new CreateAccountRequest
        //    {
        //        Email = "error@test.com",
        //        Password = "SecurePass123!"
        //    };

        //    _mockGraphService
        //        .Setup(x => x.UserExistsAsync(request.Email))
        //        .ThrowsAsync(new Exception("Unexpected database error"));

        //    // Act
        //    var result = await _controller.Post(request);

        //    // Assert
        //    var statusCodeResult = Assert.IsType<ObjectResult>(result);
        //    Assert.Equal(500, statusCodeResult.StatusCode);
        //    var response = Assert.IsType<CreateAccountResponse>(statusCodeResult.Value);
        //    Assert.False(response.Success);
        //    Assert.Equal("An unexpected error occurred while creating your account", response.Message);
        //}

        //[Theory]
        //[InlineData("Abc123!@", true)]  // Has uppercase, lowercase, number, special char
        //[InlineData("abcdefg", false)]  // Only lowercase
        //[InlineData("ABCDEFG", false)]  // Only uppercase
        //[InlineData("Abcdefg", false)]  // Only upper and lowercase
        //[InlineData("Abc1234", true)]   // Has uppercase, lowercase, number
        //[InlineData("ABC123!", true)]   // Has uppercase, number, special char
        //[InlineData("abc123!", true)]   // Has lowercase, number, special char
        //[InlineData("", true)]          // Empty (handled by Required attribute)
        //public void PasswordStrengthAttribute_ValidatesCorrectly(string password, bool shouldBeValid)
        //{
        //    // Arrange
        //    var attribute = new CreateAccountController.PasswordStrengthAttribute();
        //    var context = new System.ComponentModel.DataAnnotations.ValidationContext(new object());

        //    // Act
        //    var result = attribute.IsValid(password, context);

        //    // Assert
        //    if (shouldBeValid)
        //    {
        //        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        //    }
        //    else
        //    {
        //        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
        //    }
        //}
    }

    // End-to-end integration tests
    public class CreateAccountControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CreateAccountControllerIntegrationTests(WebApplicationFactory<Program> factory)
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

                    // Add test double
                    services.AddSingleton<IGraphService, InMemoryGraphService>();
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Post_CreateNewAccount_Returns200WithSuccessResponse()
        {
            // Act
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/createaccount", new CreateAccountRequest
                {
                    Email = "newintegration@test.com",
                    Password = "IntegrationPass123!",
                    PasswordConfirm = "IntegrationPass123!"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Account created successfully", result.Message);
            Assert.NotNull(result.UserId);
        }

        [Fact]
        public async Task Post_DuplicateEmail_Returns409Conflict()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/createaccount", new CreateAccountRequest
                {
                    Email = "valid@test.com", // Already exists in InMemoryGraphService
                    Password = "ValidPass123!",
                    PasswordConfirm = "ValidPass123!"
            });

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("An account with this Email already exists", result.Message);
        }
    }
}
