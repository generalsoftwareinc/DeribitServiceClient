using System.Text.Json.Serialization;

namespace Deribit.ApiClient.DTOs.Ticker
{
    public record TickerStats
    {
        [JsonPropertyName("high")]
        public double HighestPrice { get; set; }

        [JsonPropertyName("low")]
        public double LowestPrice { get; set; }

        [JsonPropertyName("price_change")]
        public double? PriceChange { get; set; }

        [JsonPropertyName("volume")]
        public double Volume { get; set; }

        [JsonPropertyName("volume_usd")]
        public double? VolumeUsd { get; set; }
    }
}
