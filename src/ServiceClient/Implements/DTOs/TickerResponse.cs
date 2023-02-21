namespace ServiceClient.Implements.DTOs;
using System.Text.Json.Serialization;

public class TickerResponse : SubscriptionResponse<SubscriptionParameters<TickerData>>
{
}

public class TickerData
{
    [JsonPropertyName("ask_iv")]
    public double AskImpliedVolatility { get; set; }

    [JsonPropertyName("best_ask_amount")]
    public double BestAskAmount { get; set; }

    [JsonPropertyName("best_ask_price")]
    public double? BestAskPrice { get; set; }

    [JsonPropertyName("best_bid_amount")]
    public double BestBidAmount { get; set; }

    [JsonPropertyName("best_bid_price")]
    public double? BestBidPrice { get; set; }

    [JsonPropertyName("bid_iv")]
    public double? BidIv { get; set; }

    [JsonPropertyName("current_funding")]
    public double? CurrentFunding { get; set; }

    [JsonPropertyName("delivery_price")]
    public double? DeliveryPrice { get; set; }

    [JsonPropertyName("estimated_delivery_price")]
    public double EstimatedDeliveryPrice { get; set; }

    [JsonPropertyName("funding_8h")]
    public double Funding8h { get; set; }

    [JsonPropertyName("greeks")]
    public object? Greeks { get; set; }

    [JsonPropertyName("index_price")]
    public double? IndexPrice { get; set; }

    [JsonPropertyName("instrument_name")]
    public string InstrumentName { get; set; } = string.Empty;

    [JsonPropertyName("interest_rate")]
    public double? InterestRate { get; set; }

    [JsonPropertyName("last_price")]
    public double LastPrice { get; set; }

    [JsonPropertyName("mark_iv")]
    public double? MarkIv { get; set; }

    [JsonPropertyName("mark_price")]
    public double MarkPrice { get; set; }

    [JsonPropertyName("max_price")]
    public double MaxPrice { get; set; }

    [JsonPropertyName("min_price")]
    public double MinPrice { get; set; }

    [JsonPropertyName("open_interest")]
    public double OpenInterest { get; set; }

    [JsonPropertyName("settlement_price")]
    public double SettlementPrice { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("stats")]
    public TickerStats Stats { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public long TimeStamp { get; set; }

    [JsonPropertyName("underlying_index")]
    public long? UnderlyingIndex { get; set; }

    [JsonPropertyName("underlying_price")]
    public long? UnderlyingPrice { get; set; }

    public override string ToString()
    {
        return $"Instrument: {InstrumentName}, State: {State}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, BestAskPrice: {BestAskPrice}, BestBidPrice: {BestBidPrice}";
    }
}

public class TickerStats
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