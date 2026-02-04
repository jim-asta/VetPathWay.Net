using System.Text.Json.Serialization;

namespace VetFlow.Server.DTOs
{
    public class Token
    {
        // Need JsonPropertyName because the JSON keys use snake_case but C# uses PascalCase (must match JSON exactly)
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
        [JsonPropertyName("ext_expires_in")]
        public int? ExtExpiresIn { get; set; }
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        // Error fields that might be returned instead
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
        [JsonPropertyName("error_codes")]
        public int[]? ErrorCodes { get; set; }
    }

    public class GoogleTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public string scope { get; set; } = string.Empty;
    }

    public class MicrosoftTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public string scope { get; set; } = string.Empty;
    }

}
