using Microsoft.Graph.Models;

namespace VetFlow.Server.Services
{
    public interface IGraphService
    {
        Task<User?> CreateUserAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
        Task<string?> GetUserPrincipalNameByEmailAsync(string email);
        Task<(string? DisplayName, string? Email)> GetUserNameAndEmailByPrincipalNameAsync(string userPrincipalName);
        Task<(string? DisplayName, string? Email)> GetCurrentUserNameAndEmailAsync();
    }
}