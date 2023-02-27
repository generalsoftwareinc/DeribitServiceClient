using Deribit.ServiceClient.DTOs.Book;
using Deribit.ServiceClient.DTOs.Ticker;

namespace Deribit.ServiceClient;


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
