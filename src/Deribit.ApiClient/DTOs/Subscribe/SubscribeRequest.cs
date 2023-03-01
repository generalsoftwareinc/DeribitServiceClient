using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs.Subscribe
{
    internal record SubscribeRequest
    {
        [JsonPropertyName("channels")]
        public string[] Channels { get; init; }
    }
}
