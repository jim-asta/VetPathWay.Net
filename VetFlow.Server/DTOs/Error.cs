namespace VetFlow.Server.DTOs
{
    // Error Response (for standardized error handling)
    public class ErrorResponse
    {
        public string Error { get; set; } = "";
        public string ErrorDescription { get; set; } = "";
    }
}
