using Deribit.ApiClient.DTOs.Book;
using Deribit.ApiClient.DTOs.Ticker;

namespace Deribit.ApiClient;


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
