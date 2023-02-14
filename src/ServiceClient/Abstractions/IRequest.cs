using System.Text.Json.Serialization;

namespace ServiceClient.Abstractions
{
    public interface IRequest
    {
        public long Id { get; set; }

        [JsonPropertyName("jsonrpc")]
        public string JsonRpcVersion { get; set; }

        public string Method { get; set; }

        [JsonPropertyName("params")]
        public object Parameters { get; set; }
    }
}
