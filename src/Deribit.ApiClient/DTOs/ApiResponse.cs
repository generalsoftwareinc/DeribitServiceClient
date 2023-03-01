using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs;

public record Response
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
}