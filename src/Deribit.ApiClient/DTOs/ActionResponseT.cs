using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs;

/// <summary>
/// Response DTO of action requests (like auth, hearthbeat, subscribe, ...).
/// </summary>
/// <typeparam name="T"></typeparam>
public record ActionResponse<T> where T : class
{
    /// <summary>
    /// The id that was sent in the request
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// The JSON-RPC version (2.0)
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; init; } = "";
    
    /// <summary>
    /// Action-specific result DTO
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; init; }
}