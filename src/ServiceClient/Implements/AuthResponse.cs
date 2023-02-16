using System.Text.Json.Serialization;

namespace ServiceClient.Implements;

public class AuthResponse : Response
{
    [JsonPropertyName("result")]
    public Dictionary<string, object> Result { get; set; }
}
