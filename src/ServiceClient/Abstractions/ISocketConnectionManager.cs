using ServiceClient.DTOs;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace ServiceClient.Abstractions;

public interface IDeribitSocketConnection
{
    ClientWebSocket? ClientWebSocket { get; }
    Task SocketConnectAsync(Credential credential);

    Task SocketDisconnectAsync();
}
public interface IDeribitChannelSubscription
{ 
    Task BookInstrumentIntervalSusbcribe(ClientWebSocket ClientWebSocket, Action<BookDto> onBookUpdate );
    Task TickerInstrumentIntervalSusbcribe(ClientWebSocket ClientWebSocket,  Action<TickerDto> onTickerUpdate);
}
public interface IChannelDataTransfer
{
    Task SendAsync(Channel channel);

    Task ReceiveAsync(Channel channel);
}
