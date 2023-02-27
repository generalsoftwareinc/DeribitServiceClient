using System.Text.Json.Serialization;

namespace Deribit.ServiceClient.DTOs;

public record Response
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
}