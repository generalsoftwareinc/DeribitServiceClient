using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs.Auth;

/// <summary>
/// Result DTO of responses for auth requests.
/// </summary>
internal record AuthResult
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Can be used to request a new token (with a new lifetime)
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Authorization type, allowed value - bearer
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    /// <summary>
    /// Type of the access for assigned token
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; init; } = string.Empty;

    /// <summary>
    /// Token lifetime in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; init; }
}
