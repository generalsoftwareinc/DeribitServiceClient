using System.Text.Json;
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

        public DeribitServiceClient(IDeribitSocketConnection socketConnection, ISocketDataTransfer socketDataTransfer, IOptions<DeribitOptions> deribitOptions)
        {
            this.socketConnection = socketConnection;
            this.socketDataTransfer = socketDataTransfer;
            this.deribitOptions = deribitOptions.Value;
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

        public async Task<bool> Authenticate(CancellationToken cancellationToken)
        {
            try
            {
                var request = new Request
                {
                    Id = 100,
                    Method = "public/auth",
                    Parameters = new
                    {
                        grant_type = "client_credentials",
                        client_id = deribitOptions.ApiKey,
                        client_secret = deribitOptions.ApiSecret,
                    }
                };

                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, cancellationToken);
                var result = await socketDataTransfer.ReceiveAsync<AuthResponse>(socket, cancellationToken);

                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SubscribeToChannelsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var request = new Request
                {
                    Id = 1,
                    Method = "private/subscribe",
                    Parameters = new
                    {
                        channels = new string[]
                        {
                            "book.BTC-PERPETUAL.raw" ,
                            "ticker.BTC-PERPETUAL.raw"
                        }
                    }
                };

                var socket = socketConnection.ClientWebSocket;
                var message = RequestBuilder.BuildRequest(request);
                await socketDataTransfer.SendAsync(socket, message, cancellationToken);
                var result = await socketDataTransfer.ReceiveAsync<SubscribeChannelsResponse>(socket, cancellationToken);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task ReceiveDataFromChannelsAsync(CancellationToken cancellationToken)
        {
            if (OnTickerReceived == null)
                throw new NullReferenceException(nameof(OnTickerReceived));

            var socket = socketConnection.ClientWebSocket;
            var firstBook = await ReceiveFirstBookResponse(cancellationToken);

            if (firstBook == null)
                throw new NullReferenceException(nameof(firstBook));

            var lastBook = firstBook;


            while (!cancellationToken.IsCancellationRequested)
            {
                var jsonResult = await socketDataTransfer.ReceiveAsync(socket, cancellationToken);
                var isBookChannel = jsonResult.IndexOf("book.BTC-PERPETUAL.raw") >= 0;

                if (isBookChannel)
                {
                    var book = JsonSerializer.Deserialize<BookResponse>(jsonResult);
                    if (book == null)
                        throw new NullReferenceException(nameof(book));

                    lastBook = book;
                }
                else
                {
                    var ticker = JsonSerializer.Deserialize<TickerResponse>(jsonResult);
                    if (ticker == null)
                        throw new NullReferenceException(nameof(ticker));

                    var tickerData = ticker.Parameters.Data;

                    var book = new Book
                    {
                        Asks = GetAsks(lastBook.Parameters.Data.Asks),
                        Bids = GetAsks(lastBook.Parameters.Data.Bids),
                    };

                    var tickerEventArgs = new TickerReceivedEventArgs
                    {
                        Ticker = new DTOs.Ticker
                        {
                            InstrumentName = tickerData.InstrumentName,
                            State = tickerData.State,
                            MinPrice = tickerData.MinPrice,
                            MaxPrice = tickerData.MaxPrice,
                            BestBidPrice = tickerData.BestBidPrice.Value,
                            BestAskPrice = tickerData.BestAskPrice.Value,
                            LastBook = book,
                        }
                    };

                    OnTickerReceived.Invoke(this, tickerEventArgs);
                }
            }
        }

        List<BidAskParameter> GetAsks(dynamic[] items)
        {
            var result = new List<BidAskParameter>(items.Length + 1);

            for (int i = 0; i < items.Length; i++)
            {
                var element = items[i];
                var action = (JsonElement)element[0];
                var price = (JsonElement)element[1];
                var amount = (JsonElement)element[2];

                var item = new BidAskParameter
                {
                    BidAskAction = action.GetString(),
                    Price = price.GetDouble(),
                    Amount = amount.GetDouble(),
                };

                result.Add(item);
            }

            return result;
        }

        async Task<BookResponse?> ReceiveFirstBookResponse(CancellationToken cancellationToken)
        {
            try
            {
                var socket = socketConnection.ClientWebSocket;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var jsonResult = await socketDataTransfer.ReceiveAsync(socket, cancellationToken);
                    var isBookChannel = jsonResult.IndexOf("book.BTC-PERPETUAL.raw") >= 0;

                    if (!isBookChannel)
                        continue;

                    var book = JsonSerializer.Deserialize<BookResponse>(jsonResult);
                    if (book == null)
                        throw new NullReferenceException(nameof(book));

                    return book;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
