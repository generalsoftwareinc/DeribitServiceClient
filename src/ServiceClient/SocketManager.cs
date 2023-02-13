using System.Net.WebSockets;

namespace DeribitServiceClient.ServiceClient;

public class SocketManager : ISocketManager
{
    public ClientWebSocket GetSocket()
    {
        throw new NotImplementedException();
    }

    public async Task ConnectAsync(ClientWebSocket socket)
    {
        throw new NotImplementedException();
    }
}
