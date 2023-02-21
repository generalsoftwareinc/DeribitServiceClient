namespace ServiceClient.Implements.DTOs;

using System.Text.Json.Serialization;

public class BookResponse : SubscriptionResponse<SubscriptionParameters<BookData>>
{
}

public class BookData
{
    [JsonPropertyName("type")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long TimeStamp { get; set; }

    [JsonPropertyName("prev_change_id")]
    public long PreviousChangeId { get; set; }

    [JsonPropertyName("instrument_name")]
    public string InstrumentName { get; set; } = string.Empty;

    [JsonPropertyName("change_id")]
    public ulong ChangeId { get; set; }

    [JsonPropertyName("bids")]
    public List<List<dynamic>> Bids { get; set; } = new();

    [JsonPropertyName("asks")]
    public List<List<dynamic>> Asks { get; set; } = new();

    public override string ToString()
    {
        var asks = string.Join("/", Asks.Select(a => $"{a[0]}, {a[1]}, {a[2]}"));
        var bids = string.Join("/", Bids.Select(a => $"{a[0]}, {a[1]}, {a[2]}"));
        return $"Asks: {asks}, Bids: {bids}";
    }
}
