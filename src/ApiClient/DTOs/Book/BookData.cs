using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs.Book
{
    public record BookData
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
        public List<List<dynamic>> Bids { get; set; } = new(); // TODO replace dynamic with DTO class

        [JsonPropertyName("asks")]
        public List<List<dynamic>> Asks { get; set; } = new();

        public override string ToString()
        {
            var asks = string.Join("/", Asks.Select(a => string.Join(", ", a.Select(i => i.ToString()))));
            var bids = string.Join("/", Bids.Select(a => string.Join(", ", a.Select(i => i.ToString()))));
            return $"Asks: {asks}, Bids: {bids}";
        }
    }
}
