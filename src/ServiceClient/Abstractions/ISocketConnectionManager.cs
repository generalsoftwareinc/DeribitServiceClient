using ServiceClient.DTOs;
using System.Net.WebSockets;

namespace ServiceClient.Abstractions;

public interface IDeribitSocketConnection
{
    ClientWebSocket ClientWebSocket { get; }
    Task SocketConnectAsync(CancellationToken cancellationToken);

    Task SocketDisconnectAsync(CancellationToken cancellationToken);
}
public interface IDeribitChannelSubscription
{ 
    Task BookInstrumentIntervalSusbcribe(ClientWebSocket ClientWebSocket, Action<Book> onBookUpdate );
    Task TickerInstrumentIntervalSusbcribe(ClientWebSocket ClientWebSocket,  Action<Ticker> onTickerUpdate);
}
public interface ISocketDataTransfer
{
    Task SendAsync(ClientWebSocket socket, string request, CancellationToken cancellationToken);

    Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellationToken);
}
