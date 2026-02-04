using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;

namespace VetFlow.Server.Services
{
    public class GraphService
    {
        private readonly GraphServiceClient _graphClient;
        private readonly IConfiguration _config;

        public GraphService(IConfiguration config)
        {
            _config = config;

            var clientSecretCredential = new ClientSecretCredential(
                tenantId: config["AzureAd:TenantId"],
                clientId: config["AzureAd:ClientId"],
                clientSecret: config["AzureAd:ClientSecret"]
            );

            _graphClient = new GraphServiceClient(clientSecretCredential);
        }

        public async Task<User?> CreateUserAsync(string email, string password)
        {
            try
            {
                // Use the email to create a universal user
                var emailPrefix = email.Replace("@", "_").ToLower() + "#EXT#";

                var user = new User
                {
                    AccountEnabled = true,
                    DisplayName = email.Split('@')[0],
                    MailNickname = emailPrefix,
                    UserPrincipalName = emailPrefix + _config["AzureAd:VerifiedDomain"] ?? throw new InvalidOperationException("VerifiedDomain not configured"),
                    Mail = email,
                    OtherMails = [email],
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = password
                    },
                };
                return await _graphClient.Users.PostAsync(user);
            }
            catch (Exception)       // Not handling exceptions yet, relies on null-checking for consumers of the service
            {
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                var users = await _graphClient.Users
                    .GetAsync(requestConfig =>
                    {
                        requestConfig.QueryParameters.Filter = "mail eq '" + email + "' or userPrincipalName eq '" + email + "'";
                        requestConfig.QueryParameters.Select = ["id"];
                    });
                return users?.Value?.Count > 0;
            }
            catch (Exception)       // Not handling exceptions yet, relies on null-checking for consumers of the service
            {
                return false;
            }
        }

        public async Task<string?> GetUserPrincipalNameByEmailAsync(string email)
        {
            try
            {
                var users = await _graphClient.Users
                    .GetAsync(requestConfig =>
                    {
                        requestConfig.QueryParameters.Filter = "mail eq '" + email + "' or otherMails/any(c:c eq '" + email + "')";
                        requestConfig.QueryParameters.Select = ["userPrincipalName"];
                    });
                return users?.Value?.FirstOrDefault()?.UserPrincipalName;
            }
            catch (Exception)       // Not handling exceptions yet, relies on null-checking for consumers of the service
            {
                return null;
            }
        }

        public async Task<(string? DisplayName, string? Email)> GetUserNameAndEmailByPrincipalNameAsync(string userPrincipalName)
        {
            try
            {
                var user = await _graphClient.Users[userPrincipalName]
                    .GetAsync(requestConfig =>
                    {
                        requestConfig.QueryParameters.Select = ["displayName", "mail", "userPrincipalName"];
                    });

                // Fall back to userPrincipalName if mail is null
                var email = user?.Mail ?? user?.UserPrincipalName;

                return (user?.DisplayName, email);
            }
            catch (Exception)       // Not handling exceptions yet, relies on null-checking for consumers of the service
            {
                return (null, null);
            }
        }


        public async Task<(string? DisplayName, string? Email)> GetCurrentUserNameAndEmailAsync()
        {
            try
            {
                var user = await _graphClient.Me
                    .GetAsync(requestConfig =>
                    {
                        requestConfig.QueryParameters.Select = ["displayName", "mail", "userPrincipalName"];
                    });

                // Fall back to userPrincipalName if mail is null
                var email = user?.Mail ?? user?.UserPrincipalName;

                return (user?.DisplayName, email);
            }
            catch (Exception)       // Not handling exceptions yet, relies on null-checking for consumers of the service
            {
                return (null, null);
            }
        }
    }
}
