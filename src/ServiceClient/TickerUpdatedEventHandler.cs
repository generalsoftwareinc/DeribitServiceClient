using ServiceClient.Implements.DTOs;

namespace ServiceClient;


public delegate void TickerReceivedEventHandler(object? sender, TickerReceivedEventArgs e);

public class TickerReceivedEventArgs : EventArgs
{
    public TickerReceivedEventArgs(TickerData? ticker, BookData? book)
    {
        Ticker = ticker;
        Book = book;
    }

    public TickerData? Ticker { get;}
    public BookData? Book { get; }
}
