using System.ComponentModel.DataAnnotations;
using static VetFlow.Server.Controllers.auth.CreateAccountController;

namespace VetFlow.Server.DTOs
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(256, ErrorMessage = "Password must not be longer than 256 characters")]
        [PasswordStrength(ErrorMessage = "Password must contain at least 3 of the following: uppercase letters, lowercase letters, numbers, and symbols")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords must match")]
        public string PasswordConfirm { get; set; } = "";
    }

    public class CreateAccountResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? UserId { get; set; }
    }
}
