using System.Text.Json.Serialization;

namespace ServiceClient.Implements;

public class BookResponse : SubscriptionResponse<BookData>
{
}

public class BookData
{
    [JsonPropertyName("type")]
    public string DataType { get; set; }

    [JsonPropertyName("timestamp")]
    public long TimeStamp { get; set; }

    [JsonPropertyName("prev_change_id")]
    public long PreviousChangeId { get; set; }

    [JsonPropertyName("instrument_name")]
    public string InstrumentName { get; set; }

    [JsonPropertyName("change_id")]
    public ulong ChangeId { get; set; }

    [JsonPropertyName("bids")]
    //public Tuple<string, double, int>[] Bids { get; set; }
    public dynamic[] Bids { get; set; }

    [JsonPropertyName("asks")]
    public dynamic[] Asks { get; set; }
}

public class BidAskParameter
{
    [JsonPropertyOrder(0)]
    public string BidAskAction { get; set; }

    [JsonPropertyOrder(1)]
    public double Price { get; set; }

    [JsonPropertyOrder(2)]
    public double Amount { get; set; }
}
