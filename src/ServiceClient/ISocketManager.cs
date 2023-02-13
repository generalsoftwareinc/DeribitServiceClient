using System.Net.WebSockets;

namespace DeribitServiceClient.ServiceClient;

public interface ISocketManager
{
    ClientWebSocket GetSocket();
    Task ConnectAsync(ClientWebSocket socket);
}