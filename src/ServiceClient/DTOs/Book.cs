namespace ServiceClient.DTOs;

public class Book
{
    public List<(string, double, int)> Asks { get; set; } = new();
    public List<(string, double, int)> Bids { get; set; } = new();

    public override string ToString()
    {
        var asks = string.Join("/", Asks.Select(a => $"{a.Item1}, {a.Item2}, {a.Item3}"));
        var bids = string.Join("/", Bids.Select(a => $"{a.Item1}, {a.Item2}, {a.Item3}"));
        return $"Asks: {asks},Bids: {bids}";

    }
}
