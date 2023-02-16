using System.Text.Json.Serialization;

namespace ServiceClient.Implements;

public class SubscriptionResponse<T> where T : class
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpcVersion { get; set; } = "";

    [JsonPropertyName("method")]
    public string Method { get; set; } = "";

    [JsonPropertyName("params")]
    public SubscriptionParameters Parameters { get; set; } = null;

    public class SubscriptionParameters
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }
    }
}
