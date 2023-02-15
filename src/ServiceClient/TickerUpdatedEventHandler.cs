using ServiceClient.DTOs;

namespace ServiceClient;


public delegate void TickerReceivedEventHandler(object? sender, TickerReceivedEventArgs e);

public class TickerReceivedEventArgs : EventArgs
{
    public Ticker Ticker { get; set; }
}
