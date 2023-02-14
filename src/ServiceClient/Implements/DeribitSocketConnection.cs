using ServiceClient.Abstractions;
using ServiceClient.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient.Implements
{
    public class DeribitSocketConnection : IDeribitSocketConnection
    {
        private readonly WebSocketOptions options;

        private ClientWebSocket? socket;

        public DeribitSocketConnection(WebSocketOptions options) 
        { 
            this.options = options;
        }

        public ClientWebSocket ClientWebSocket
        {
            get
            {
                if (socket != null)
                    return socket;
                socket = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(options.KeepAliveIntervalInSeconds)
                    }
                };
                return socket;
            }
        }

        public async Task SocketConnectAsync(CancellationToken cancellationToken)
        {
            var url = new Uri(options.Url);

            await ClientWebSocket
                .ConnectAsync(url, cancellationToken)
                .Timeout(options.ConnectionTimeoutInMilliseconds);
        }

        public async Task SocketDisconnectAsync(CancellationToken cancellationToken)
        {
            await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", cancellationToken);
        }
    }

    public static class TaskExtensions
    {
        public static async Task Timeout(this Task task, int timeoutInMilliseconds)
        {
            if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)) != task)
                throw new TimeoutException($"timed out after {timeoutInMilliseconds} milliseconds");
        }
    }
}
