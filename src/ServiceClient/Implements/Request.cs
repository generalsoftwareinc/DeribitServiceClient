using ServiceClient.Abstractions;
using System.Text.Json.Serialization;

namespace ServiceClient.Implements
{
    internal class Request : IRequest
    {
        public long Id { get; set; }

        [JsonPropertyName("jsonrpc")]
        public string JsonRpcVersion { get; set; } = "2.0";

        public string Method { get; set; } = "";

        [JsonPropertyName("params")]
        public object Parameters { get; set; } = new object();
    }
}
