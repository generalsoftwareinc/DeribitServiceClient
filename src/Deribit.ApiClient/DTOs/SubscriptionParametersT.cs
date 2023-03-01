using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs;

public record SubscriptionParameters<T>
    where T : class
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;
}
