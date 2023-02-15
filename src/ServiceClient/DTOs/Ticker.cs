namespace ServiceClient.DTOs;

public class Ticker
{
    public string State { get; set; } = string.Empty;
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; }
    public double MarkPrice { get; set; }
    public double LastPrice { get; set; }
    public double BestBidPrice { get; set; }
    public double BestAskPrice { get; set; }
    public string InstrumentName { get; set; } = string.Empty;
    public Book LastBook { get; set; } = new();

    public override string ToString()
    {
        return $"Instrument: {InstrumentName}, State: {State}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, BestAskPrice: {BestAskPrice}, BestBidPrice: {BestBidPrice}, Book: {LastBook}";
    }
}
