namespace VetFlow.Server.DTOs
{
    public class Token
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string? scope { get; set; }
        public int? ext_expires_in { get; set; }
        public string? id_token { get; set; }
        public string? refresh_token { get; set; } // Often included

        // Error fields that might be returned instead
        public string? error { get; set; }
        public string? error_description { get; set; }
        public int[]? error_codes { get; set; }
    }
}
