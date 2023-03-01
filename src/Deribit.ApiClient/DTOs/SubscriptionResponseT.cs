using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs;

public record SubscriptionResponse<T> : Response
    where T : class
{
    [JsonPropertyName("params")]
    public T? Parameters { get; set; }
}