using System.ComponentModel.DataAnnotations;

namespace VetFlow.Server.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string? TokenType { get; set; } = "";
        public string? AccessToken { get; set; } = "";
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }
        public int? ExtExpiresIn { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
