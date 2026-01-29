using System.ComponentModel.DataAnnotations;
using static VetFlow.Server.Controllers.auth.CreateAccountController;

namespace VetFlow.Server.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";
    }

    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
