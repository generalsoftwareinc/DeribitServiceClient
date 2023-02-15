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

        public event EventHandler? OnTickerReceived;

        public async Task<bool> DisconnectAsync(CancellationToken cancellationToken)
        {
            if (!initialized) 
                return true;
            try
            {
                await socketConnection.SocketDisconnectAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool initialized = false;
        public async Task<bool> InitializeAsync(CancellationToken cancellationToken)
        {
            if (initialized) 
                return true;
            try
            {
                await socketConnection.SocketConnectAsync(cancellationToken);
                initialized = true;
            }
            catch
            {
                initialized = false;
            }
            return initialized;
        }

        public async Task<bool> IsDeribitAvailableAsync(CancellationToken cancellationToken)
        {            
            try
            {
                await InitializeAsync(cancellationToken);
                var request = new Request
                {
                    Id = 100,
                    Method = "public/test"
                };
                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, cancellationToken);
                var result = await socketDataTransfer.ReceiveAsync(socket, cancellationToken);
                return result != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
