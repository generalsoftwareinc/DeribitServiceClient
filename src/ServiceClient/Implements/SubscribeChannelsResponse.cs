using System.Text.Json.Serialization;

namespace ServiceClient.Implements;

class SubscribeChannelsResponse : Response
{
    [JsonPropertyName("result")]
    public string[] Result { get; set; }
}
