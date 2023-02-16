using ServiceClient.Implements;

namespace ServiceClient.DTOs;

public class Book
{
    public List<BidAskParameter> Asks { get; set; } = new();
    public List<BidAskParameter> Bids { get; set; } = new();

    public override string ToString()
    {
        var asks = string.Join("/", Asks.Select(a => $"{a.BidAskAction}, {a.Price}, {a.Amount}"));
        var bids = string.Join("/", Bids.Select(a => $"{a.BidAskAction}, {a.Price}, {a.Amount}"));
        return $"Asks: {asks},Bids: {bids}";

    }
}
