using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using ServiceClient.DTOs;

namespace ServiceClient.Implements
{
    public class DeribitServiceClient : IServiceClient
    {
        private readonly IDeribitSocketConnection socketConnection;
        private readonly ISocketDataTransfer socketDataTransfer;
        private readonly DeribitOptions deribitOptions;

        private const int TEST_AVAILABLE = 100;
        private const int AUTHENTICATE = 101;

        public DeribitServiceClient(IDeribitSocketConnection socketConnection, ISocketDataTransfer socketDataTransfer, IOptions<DeribitOptions> options)
        {
            this.socketConnection = socketConnection;
            this.socketDataTransfer = socketDataTransfer;
            this.deribitOptions= options.Value;
        }

        public event TickerReceivedEventHandler? OnTickerReceived;

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
                    Id = TEST_AVAILABLE,
                    Method = "public/test"
                };
                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, cancellationToken);
                var result = await socketDataTransfer.ReceiveAsync(socket, cancellationToken);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var request = new Request
                {
                    Id = AUTHENTICATE,
                    Method = "public/auth",
                    Parameters = new
                    {
                        grant_type = "client_credentials",
                        client_id = deribitOptions.ApiKey,
                        client_secret = deribitOptions.ApiSecret
                    }
                };
                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, cancellationToken);
                var result = await socketDataTransfer.ReceiveAsync(socket, cancellationToken);
                //TODO: Deserialize the cliente credentials
                return (result != null);
            }
            catch
            {
                return false;
            }
        }
    }
}
