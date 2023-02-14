using ServiceClient.Abstractions;

namespace ServiceClient.Implements
{
    public class DeribitServiceClient : IServiceClient
    {
        private readonly IDeribitSocketConnection socketConnection;
        private readonly ISocketDataTransfer socketDataTransfer;

        public DeribitServiceClient(IDeribitSocketConnection socketConnection, ISocketDataTransfer socketDataTransfer)
        {
            this.socketConnection = socketConnection;
            this.socketDataTransfer = socketDataTransfer;
        }

        public event EventHandler OnTickerReceived;

        public async Task<bool> DisconnectAsync()
        {
            if (!initialized) 
                return true;
            try
            {
                await socketConnection.SocketDisconnectAsync(CancellationToken.None);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool initialized = false;
        public async Task<bool> InitializeAsync()
        {
            if (initialized) 
                return true;
            await socketConnection.SocketConnectAsync(CancellationToken.None);
            initialized = true;
            return initialized;
        }

        public async Task<bool> IsDeribitAvailableAsync()
        {            
            try
            {
                await InitializeAsync();
                var request = new Request
                {
                    Id = 100,
                    Method = "public/test"
                };
                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, CancellationToken.None);
                var result = await socketDataTransfer.ReceiveAsync(socket, CancellationToken.None);
                return result != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
