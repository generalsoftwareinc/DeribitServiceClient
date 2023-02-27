using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Deribit.ServiceClient.DTOs.Ticker
{
    public record TickerData
    {
        [JsonPropertyName("ask_iv")]
        public double AskImpliedVolatility { get; init; }

        [JsonPropertyName("best_ask_amount")]
        public double BestAskAmount { get; init; }

        [JsonPropertyName("best_ask_price")]
        public double? BestAskPrice { get; init; }

        [JsonPropertyName("best_bid_amount")]
        public double BestBidAmount { get; init; }

        [JsonPropertyName("best_bid_price")]
        public double? BestBidPrice { get; init; }

        [JsonPropertyName("bid_iv")]
        public double? BidIv { get; init; }

        [JsonPropertyName("current_funding")]
        public double? CurrentFunding { get; init; }

        [JsonPropertyName("delivery_price")]
        public double? DeliveryPrice { get; init; }

        [JsonPropertyName("estimated_delivery_price")]
        public double EstimatedDeliveryPrice { get; init; }

        [JsonPropertyName("funding_8h")]
        public double Funding8h { get; init; }

        [JsonPropertyName("greeks")]
        public object? Greeks { get; init; }

        [JsonPropertyName("index_price")]
        public double? IndexPrice { get; init; }

        [JsonPropertyName("instrument_name")]
        public string InstrumentName { get; init; } = string.Empty;

        [JsonPropertyName("interest_rate")]
        public double? InterestRate { get; init; }

        [JsonPropertyName("last_price")]
        public double LastPrice { get; init; }

        [JsonPropertyName("mark_iv")]
        public double? MarkIv { get; init; }

        [JsonPropertyName("mark_price")]
        public double MarkPrice { get; init; }

        [JsonPropertyName("max_price")]
        public double MaxPrice { get; init; }

        [JsonPropertyName("min_price")]
        public double MinPrice { get; init; }

        [JsonPropertyName("open_interest")]
        public double OpenInterest { get; init; }

        [JsonPropertyName("settlement_price")]
        public double SettlementPrice { get; init; }

        [JsonPropertyName("state")]
        public string State { get; init; } = string.Empty;

        [JsonPropertyName("stats")]
        public TickerStats Stats { get; init; } = new();

        [JsonPropertyName("timestamp")]
        public long TimeStamp { get; init; }

        [JsonPropertyName("underlying_index")]
        public long? UnderlyingIndex { get; init; }

        [JsonPropertyName("underlying_price")]
        public long? UnderlyingPrice { get; init; }

        public override string ToString()
        {
            return $"Instrument: {InstrumentName}, State: {State}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, BestAskPrice: {BestAskPrice}, BestBidPrice: {BestBidPrice}";
        }
    }
}
