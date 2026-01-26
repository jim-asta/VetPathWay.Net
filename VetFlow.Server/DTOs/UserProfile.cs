namespace VetFlow.Server.DTOs
{
    public class UserProfile
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime LastLogin { get; set; }
        public int TotalLogins { get; set; }
        public bool IsActive { get; set; }
    }
}
