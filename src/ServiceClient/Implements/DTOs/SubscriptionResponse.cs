using System.Text.Json.Serialization;

namespace ServiceClient.Implements.DTOs;

public class Response
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
}

public class ActionResponse<T> where T : class
{
    [JsonPropertyName("result")]
    public T? Result { get; set; }
}

public class SubscriptionResponse<T> : Response
{
    [JsonPropertyName("params")]
    public T? Parameters { get; set; }
}

public class SubscriptionParameters<T> where T : class
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;
}
