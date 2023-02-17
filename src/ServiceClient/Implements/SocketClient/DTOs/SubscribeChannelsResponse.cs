using System.Text.Json.Serialization;

namespace ServiceClient.Implements.SocketClient.DTOs;

class SubscribeChannelsResponse : Response
{
    [JsonPropertyName("result")]
    public string[] Result { get; set; }
}
