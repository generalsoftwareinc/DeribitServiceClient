using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs.Auth
{
    internal record AuthRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; init; }

        [JsonPropertyName("client_id")]
        public string? ClientId { get; init; }

        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }
    }
}
