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
using System.Threading.Channels;
using Websocket.Client;

namespace Deribit.ServiceClient;

internal partial class DeribitApiClient : IDeribitApiClient
{
    private const string EndMessage = "END";
    
    private int nextId = 0;

    private readonly Channel<string> incomingQueue;
    private readonly Channel<string> outgoingQueue;

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

        incomingQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true }); // TODO debug and optimize whether SingleWriter can be true
        outgoingQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true }); // TODO debug and optimize whether SingleWriter can be true
    }

    public event TickerReceivedEventHandler? OnTickerReceived;

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        EnqueueOutgoingMessage(EndMessage, CancellationToken.None);

        return Task.CompletedTask;
    }

    protected ClientWebSocket WsFactory() => new()
    {
        Options =
        {
            KeepAliveInterval = TimeSpan.Zero
        }
    };

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // create the socket worker
        var socketWorker = Task.Run(() => DeribitSocketWorker(this.options, cancellationToken), cancellationToken);

        // send messages to connect, auth and subscribe
        EnqueueOutgoingMessage(GetTestRequestMessage(), cancellationToken);

        // wait until incoming messages become available
        while (await incomingQueue.Reader.WaitToReadAsync(cancellationToken))
        {
            // take the next available incoming message
            while (incomingQueue.Reader.TryRead(out string? message))
            {
                try
                {
                    ParseMessage(message);
                }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Failed processing received message: {message}", message);
                }
            }
        }

        await socketWorker;
    }

    private void RefreshTokenTimerTicked(object? state)
    {
        refreshTokenTimer?.Dispose();

        // TODO handle the case when Credentials is null!

        var message = GetAuthenticateByRefreshTokenMessage(Credentials!.RefreshToken);
        EnqueueOutgoingMessage(message, CancellationToken.None);
    }

    async Task DeribitSocketWorker(DeribitOptions options, CancellationToken token)
    {
        // setup new socket worker
        using var ws = new WebsocketClient(new Uri(options.WebSocketUrl), WsFactory);
        ws.MessageReceived.Subscribe(info =>
        {
            if (!string.IsNullOrWhiteSpace(info.Text))
                incomingQueue.Writer.TryWrite(info.Text);
        });
        ws.ReconnectionHappened.Subscribe(info =>
        {
            // re-authenticate on reconnect
            EnqueueConnectionInitMessagesToSend(options, token);
        });

        // start receiving messages
        await ws.Start();

        // wait until outgoing messages become available
        while (await outgoingQueue.Reader.WaitToReadAsync(token))
        {
            // take the next available incoming message
            while (outgoingQueue.Reader.TryRead(out string? message))
            {
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
    }

    private void EnqueueConnectionInitMessagesToSend(DeribitOptions options, CancellationToken token)
    {
        EnqueueOutgoingMessage(GetAuthenticateByClientCredentialsMessage(options), token);
        EnqueueOutgoingMessage(GetSubscribeMessage(options), token);
        EnqueueOutgoingMessage(GetSetHeartBeatMessage(), token);
    }

    private void EnqueueOutgoingMessage(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!outgoingQueue.Writer.TryWrite(message)) // not async because the channel is unbounded
        {
            // should not happen if channel is unbounded
            this.logger?.LogError("Could not write to outgoing queue: {message}", message);
        }
    }
}
