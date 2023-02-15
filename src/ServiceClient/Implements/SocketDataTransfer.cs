using ServiceClient.Abstractions;
using System.Net.WebSockets;
using System.Text;

namespace ServiceClient.Implements
{
    public class SocketDataTransfer : ISocketDataTransfer
    {
        private const int READ_BUFFER_SIZE = 4096;

        private StringBuilder sb = new StringBuilder();
        private byte[]? buffer;

        public async Task<string> ReceiveAsync(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            sb.Clear();
            buffer = new byte[READ_BUFFER_SIZE];
            WebSocketReceiveResult response;
            do
            {
                response = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var result = Encoding.Default.GetString(buffer, 0, response.Count);

                sb.Append(result);
            }
            while (!response.EndOfMessage);
            buffer = null;
            return sb.ToString();
        }

        public async Task SendAsync(ClientWebSocket socket, string message, CancellationToken cancellationToken)
        {
            buffer = new byte[message.Length];
            var length = Encoding.Default.GetBytes(message, 0, message.Length, buffer, 0);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
