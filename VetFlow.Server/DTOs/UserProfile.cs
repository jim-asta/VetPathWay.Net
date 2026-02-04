namespace VetFlow.Server.DTOs
{
    public class UserProfileResponse
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime LastLogin { get; set; }
        public int TotalLogins { get; set; }
        public bool IsActive { get; set; }
    }

    public class GoogleUserInfo
    {
        public string sub { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool email_verified { get; set; }
        public string name { get; set; } = string.Empty;
        public string? picture { get; set; }
        public string given_name { get; set; } = string.Empty;
        public string family_name { get; set; } = string.Empty;
    }

    public class MicrosoftUserInfo
    {
        public string sub { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? preferred_username { get; set; }
    }
}
