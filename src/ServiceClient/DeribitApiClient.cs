using Deribit.ServiceClient.Abstractions;
using Deribit.ServiceClient.Configuration;
using Deribit.ServiceClient.DTOs;
using Deribit.ServiceClient.DTOs.Auth;
using Deribit.ServiceClient.DTOs.Book;
using Deribit.ServiceClient.DTOs.Subscribe;
using Deribit.ServiceClient.DTOs.Ticker;
using Deribit.ServiceClient.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;

namespace Deribit.ServiceClient;

internal partial class DeribitApiClient : IDeribitApiClient
{
    private const string EndMessage = "END";
    
    private int nextId = 0;
    static protected readonly BlockingCollection<string> readMessageQueue = new();
    static protected readonly BlockingCollection<string> sendMessageQueue = new();

    private readonly RequestParameterFactory requestParameterFactory;
    private readonly DeribitOptions options;
    private readonly ILogger logger;

    private BookData? lastBook;
    private Timer? refreshTokenTimer;
    
    public AuthResult? Credentials { get; protected set; }

    public DeribitApiClient(IOptions<DeribitOptions> deribitOptions, ILogger<DeribitApiClient> logger)
    {
        this.options = deribitOptions.Value;
        this.logger = logger;

        this.requestParameterFactory = new RequestParameterFactory(); // TODO should be injected
    }

    public long TickerMessagesCount { get; private set; }
    public long BookMessagesCount { get; private set; }

    public event TickerReceivedEventHandler? OnTickerReceived;

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        sendMessageQueue.Add(EndMessage);

        return Task.CompletedTask;
    }

    protected ClientWebSocket WsFactory() => new()
    {
        Options =
        {
            KeepAliveInterval = TimeSpan.Zero
        }
    };

    public Task RunAsync(CancellationToken cancellationToken)
    {
        // create the socket worker
        var socketWorker = Task.Run(() => DeribitSocketWorker(this.options, cancellationToken), cancellationToken);

        // send messages to connect, auth and subscribe
        sendMessageQueue.Add(GetTestRequestMessage(), cancellationToken);
        EnqueueConnectionInitMessagesToSend(this.options, cancellationToken);

        // read incoming messages until asked to stop
        while (!cancellationToken.IsCancellationRequested)
        {
            // take the next available incoming message or wait until one gets added
            var message = readMessageQueue.Take(cancellationToken);

            try
            {
                ParseMessage(message);
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "Failed processing received message: {message}", message);
            }
        }

        return socketWorker;
    }

    private void RefreshTokenTimerTicked(object? state)
    {
        refreshTokenTimer?.Dispose();

        // TODO handle the case when Credentials is null!

        var message = GetAuthenticateByRefreshTokenMessage(Credentials!.RefreshToken);
        sendMessageQueue.Add(message);
    }

    async Task DeribitSocketWorker(DeribitOptions options, CancellationToken token)
    {
        // setup new socket worker
        using var ws = new WebsocketClient(new Uri(options.WebSocketUrl), WsFactory);
        ws.MessageReceived.Subscribe(info => readMessageQueue.Add(info.Text));
        ws.ReconnectionHappened.Subscribe(info =>
        {
            if (info.Type != ReconnectionType.Initial)
            {
                // re-authenticate on reconnect
                EnqueueConnectionInitMessagesToSend(options, token);
            }
        });

        // start receiving messages
        await ws.Start();

        // send messages in queue until we're asked to stop
        while (!token.IsCancellationRequested)
        {
            // take the next available outgoing message or wait until one gets added
            var message = sendMessageQueue.Take(token);

            // if we have to stop
            if (message == EndMessage)
            {
                // close the WebSocket
                await ws.Stop(WebSocketCloseStatus.NormalClosure, message);
                break;
            }

            // send the message through the WebSocket
            await ws.SendInstant(message);
        }
    }

    private void EnqueueConnectionInitMessagesToSend(DeribitOptions options, CancellationToken token)
    {
        sendMessageQueue.Add(GetAuthenticateByClientCredentialsMessage(options), token);
        sendMessageQueue.Add(GetSubscribeMessage(options), token);
        sendMessageQueue.Add(GetSetHeartBeatMessage(), token);
    }
}
