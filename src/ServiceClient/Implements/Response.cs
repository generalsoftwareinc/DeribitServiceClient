using System.Text.Json.Serialization;

namespace ServiceClient.Implements;

public class Response
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; set; } = "";

    [JsonPropertyName("usIn")]
    public ulong UsIn { get; set; }

    [JsonPropertyName("usOut")]
    public ulong UsOut { get; set; }

    [JsonPropertyName("usDiff")]
    public int UsDiff { get; set; }

    [JsonPropertyName("error")]
    public ResponseError? Error { get; set; } = null;
}

public class ResponseError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    /// <summary>
    /// Per official documentation:
    /// data: any type
    /// Additional data about the error. This field may be omitted.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}