using Microsoft.Graph.Models;
using VetFlow.Server.Services;

namespace VetFlow.Server.Tests.Controllers
{
    public class InMemoryGraphService : IGraphService
    {
        private readonly Dictionary<string, string> _userMappings = new()
            {
                { "valid@test.com", "valid@test.onmicrosoft.com" },
                { "user@test.com", "user@test.onmicrosoft.com" },
                { "admin@test.com", "admin@test.onmicrosoft.com" }
            };

        private readonly Dictionary<string, (string DisplayName, string Email)> _userDetails = new()
            {
                { "valid@test.onmicrosoft.com", ("Valid User", "valid@test.com") },
                { "user@test.onmicrosoft.com", ("Test User", "user@test.com") },
                { "admin@test.onmicrosoft.com", ("Admin User", "admin@test.com") }
            };

        public Task<string?> GetUserPrincipalNameByEmailAsync(string email)
        {
            _userMappings.TryGetValue(email, out var upn);
            return Task.FromResult(upn);
        }

        public Task<(string? DisplayName, string? Email)> GetUserNameAndEmailByPrincipalNameAsync(string userPrincipalName)
        {
            if (_userDetails.TryGetValue(userPrincipalName, out var details))
                return Task.FromResult<(string?, string?)>((details.DisplayName, details.Email));

            return Task.FromResult<(string?, string?)>((null, null));
        }

        public Task<(string? DisplayName, string? Email)> GetCurrentUserNameAndEmailAsync()
        {
            // For testing, return a default current user
            return Task.FromResult<(string?, string?)>(("Current Test User", "current@test.com"));
        }

        public Task<bool> UserExistsAsync(string email)
        {
            var exists = _userMappings.ContainsKey(email);
            return Task.FromResult(exists);
        }

        public Task<User?> CreateUserAsync(string email, string displayName)
        {
            // For testing, just simulate success
            var upn = email.Split('@')[0] + "@test.onmicrosoft.com";
            _userMappings[email] = upn;
            _userDetails[upn] = (displayName, email);
            return Task.FromResult<User?>(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserPrincipalName = upn,
                DisplayName = displayName,
                Mail = email
            });
        }
    }
}
