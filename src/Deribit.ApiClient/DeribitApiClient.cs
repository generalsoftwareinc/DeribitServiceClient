using Deribit.ApiClient.Abstractions;
using Deribit.ApiClient.Configuration;
using Deribit.ApiClient.DTOs.Auth;
using Deribit.ApiClient.DTOs.Book;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Threading.Channels;
using Websocket.Client;

namespace Deribit.ApiClient;

public partial class DeribitApiClient : IDeribitApiClient, IAsyncDisposable, IDisposable
{
    private const string EndMessage = "END";
    
    private int nextId = 0;
    private bool isDisposingOrDisposed;
    private bool isProcessingOutgoingMessages;
    private bool isProcessingIncomingMessages;

    private readonly Channel<string> incomingQueue;
    private readonly Channel<string> outgoingQueue;

    private readonly RequestParameterFactory requestParameterFactory;
    private readonly DeribitOptions options;
    private readonly ILogger? logger;

    private BookData? lastBook;
    private Timer? refreshTokenTimer;
    //private Task? runningTask = null;
    private CancellationTokenSource? stopTokenSource;
    
    private AuthResult? Credentials { get; set; }

    public bool IsRunning { get; private set; }

    public DeribitApiClient(IOptions<DeribitOptions> deribitOptions, ILogger<DeribitApiClient>? logger)
    {
        if (deribitOptions == null)
            throw new ArgumentNullException(nameof(deribitOptions));

        this.options = deribitOptions.Value;
        this.logger = logger;

        this.requestParameterFactory = new RequestParameterFactory(); // TODO should be injected

        incomingQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true }); // TODO debug and optimize whether SingleWriter can be true
        outgoingQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true }); // TODO debug and optimize whether SingleWriter can be true
    }

    public event TickerReceivedEventHandler? OnTickerReceived;

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposingOrDisposed();

        if (this.IsRunning)
        {
            this.logger?.LogDebug("Disconnecting");
            EnqueueOutgoingMessage(EndMessage, CancellationToken.None);
            this.stopTokenSource?.Cancel();
        }

        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken externalRunCancellationToken)
    {
        ThrowIfDisposingOrDisposed();

        if (this.IsRunning)
            throw new InvalidOperationException("Already running.");

        Task? socketWorker = null;

        try
        {
            this.logger?.LogDebug("Starting Deribit websocket worker");
            this.stopTokenSource = new CancellationTokenSource();
            this.IsRunning = true;
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalRunCancellationToken, this.stopTokenSource.Token);

            // create the socket worker
            socketWorker = Task.Run(() => DeribitSocketWorker(this.options, linkedCts.Token), linkedCts.Token);

            //this.runningTask = socketWorker;

            // send messages to connect, auth and subscribe
            EnqueueOutgoingMessage(GetTestRequestMessage(), linkedCts.Token);

            this.logger?.LogDebug("Start processing incoming messages");
            isProcessingIncomingMessages = true;

            // wait until incoming messages become available
            while (!isDisposingOrDisposed && await incomingQueue.Reader.WaitToReadAsync(linkedCts.Token))
            {
                if (incomingQueue.Reader.CanCount)
                    this.logger?.LogTrace("Processing {count} received messages", incomingQueue.Reader.Count);
                else
                    this.logger?.LogTrace("Processing received messages");

                // take the next available incoming message
                while (!isDisposingOrDisposed && incomingQueue.Reader.TryRead(out string? message))
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

            isProcessingIncomingMessages = false;

            if (isDisposingOrDisposed)
                this.logger?.LogDebug("Cancelled processing incoming message queue by dispose");
            else if (externalRunCancellationToken.IsCancellationRequested)
                this.logger?.LogDebug("Cancelled processing incoming message queue by external cancel token");
            else if (stopTokenSource.IsCancellationRequested)
                this.logger?.LogDebug($"Cancelled processing incoming message queue by {nameof(DisconnectAsync)}");
            else
                this.logger?.LogDebug("Incoming message queue completed.");

            // wait until socket worker stops itself
            await socketWorker;
        }
        catch (OperationCanceledException)
        {
            this.logger?.LogInformation("Api client cancelled.");
            if (socketWorker != null && !socketWorker.IsCanceled && !socketWorker.IsCompleted)
                await socketWorker;
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Failed Initializing or processing incoming messages.");
            throw;
        }
        finally
        {
            if (isProcessingIncomingMessages)
                isProcessingIncomingMessages = false;
            DisposeRefreshTokenTimer();
            this.logger?.LogDebug("Deribit websocket worker stopped");

            //this.runningTask = null;

            // TODO here it must be true that neither incoming, nor outgoing message processing should be running!

            RefreshIsRunning();
        }
    }

    public void Reset()
    {
        ThrowIfDisposingOrDisposed();

        nextId = 0;
        lastBook = null;
        Credentials = null;

        this.TotalReceivedMessagesCount = 0;
        this.BookMessagesCount = 0;
        this.HeartBeatMessagesCount = 0;
        this.SubscriptionMessagesCount = 0;
        this.TickerMessagesCount = 0;
        this.TokenRefreshMessagesCount = 0;

        // TODO implement
    }

    private static ClientWebSocket WsFactory() => new()
    {
        Options =
        {
            KeepAliveInterval = TimeSpan.Zero
        }
    };

    private void RefreshIsRunning()
    {
        this.IsRunning = isProcessingIncomingMessages || isProcessingOutgoingMessages;
    }

    private async Task DeribitSocketWorker(DeribitOptions options, CancellationToken token)
    {
        ThrowIfDisposingOrDisposed();

        this.logger?.LogDebug("Creating new websocket client");

        using WebsocketClient ws = CreateSocketClient(options, token);

        this.logger?.LogDebug("Starting websocket client");

        // start receiving messages
        await ws.Start();

        try
        {
            this.logger?.LogDebug("Start processing outgoing messages");
            isProcessingOutgoingMessages = true;

            // wait until outgoing messages become available
            while (!isDisposingOrDisposed && await outgoingQueue.Reader.WaitToReadAsync(token))
            {
                if (outgoingQueue.Reader.CanCount)
                    this.logger?.LogTrace("Processing {count} outgoing messages", outgoingQueue.Reader.Count);
                else
                    this.logger?.LogTrace("Processing outgoing messages");

                // take the next available incoming message
                while (!isDisposingOrDisposed && outgoingQueue.Reader.TryRead(out string? message))
                {
                    // if we have to stop
                    if (message == EndMessage || token.IsCancellationRequested)
                    {
                        // close the WebSocket
                        await ws.Stop(WebSocketCloseStatus.NormalClosure, message);
                        break;
                    }

                    // send the message through the WebSocket
                    await ws.SendInstant(message);
                }
            }

            isProcessingOutgoingMessages = false;

            if (isDisposingOrDisposed)
                this.logger?.LogDebug("Cancelled processing outgoing message queue by dispose");
            else if (token.IsCancellationRequested)
                this.logger?.LogDebug("Cancelled processing outgoing message queue by cancel token");
            else
                this.logger?.LogDebug("Outgoing message queue completed.");
        }
        catch (OperationCanceledException)
        {
            this.logger?.LogInformation("Socket worker cancelled.");

            //outgoingQueue.Writer.Complete(ex);
            var stopped = await ws.Stop(WebSocketCloseStatus.Empty, "Cancelled"); // TODO: do we need this here?
            //incomingQueue.Writer.Complete(ex);
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Failed Initializing or processing incoming messages.");

            //outgoingQueue.Writer.Complete(ex);
            var stopped = await ws.Stop(WebSocketCloseStatus.Empty, "Exception"); // TODO: do we need this here?
            //incomingQueue.Writer.Complete(ex);
        }
        finally
        {
            if (isProcessingOutgoingMessages)
                isProcessingOutgoingMessages = false;
            this.logger?.LogDebug("Websocket client stopped");

            RefreshIsRunning();
        }
    }

    private WebsocketClient CreateSocketClient(DeribitOptions options, CancellationToken token)
    {
        ThrowIfDisposingOrDisposed();
        token.ThrowIfCancellationRequested();

        WebsocketClient? ws = null;
        try
        {
            // setup new socket worker
            ws = new WebsocketClient(new Uri(options.WebSocketUrl), WsFactory);
            ws.MessageReceived.Subscribe(info =>
            {
                if (isDisposingOrDisposed || token.IsCancellationRequested) return;

                this.TotalReceivedMessagesCount++;
                this.logger?.LogTrace("Incoming message received: {message}", info.Text);

                if (!string.IsNullOrWhiteSpace(info.Text))
                    incomingQueue.Writer.TryWrite(info.Text);
            });
            ws.ReconnectionHappened.Subscribe(info =>
            {
                if (isDisposingOrDisposed || token.IsCancellationRequested) return;

                this.logger?.LogInformation("WebSocket connected. (Details: {info})", info);

                // re-authenticate on reconnect
                EnqueueConnectionInitMessagesToSend(options, token);
            });
            ws.DisconnectionHappened.Subscribe(info =>
            {
                if (isDisposingOrDisposed || token.IsCancellationRequested) return;

                this.logger?.LogInformation("WebSocket disconnected. (Details: {info})", info);
            });
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "Failed initializing new socket client.");

            ws?.Dispose();

            throw;
        }

        return ws;
    }

    private void EnqueueConnectionInitMessagesToSend(DeribitOptions options, CancellationToken token)
    {
        if (isDisposingOrDisposed)
            return;

        EnqueueOutgoingMessage(GetAuthenticateByClientCredentialsMessage(options), token);
        EnqueueOutgoingMessage(GetSubscribeMessage(options), token);
        EnqueueOutgoingMessage(GetSetHeartBeatMessage(), token);
    }

    private void EnqueueOutgoingMessage(string message, CancellationToken cancellationToken)
    {
        if (isDisposingOrDisposed)
            return;

        cancellationToken.ThrowIfCancellationRequested();

        if (outgoingQueue.Writer.TryWrite(message)) // not async because the channel is unbounded
        {
            this.logger?.LogTrace("Outgoing message added to queue: {message}", message);
        }
        else
        {
            // should not happen if channel is unbounded
            this.logger?.LogError("Could not write to outgoing queue: {message}", message);
        }
    }

    #region Dispose
    private void ThrowIfDisposingOrDisposed()
    {
        if (isDisposingOrDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        if (isDisposingOrDisposed)
            return;
        this.isDisposingOrDisposed = true;

        // https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposingOrDisposed)
            return;
        this.isDisposingOrDisposed = true;

        // https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.logger?.LogDebug("Disposing");

            DisposeRefreshTokenTimer();

            this.outgoingQueue.Writer.TryComplete();
            this.incomingQueue.Writer.TryComplete();

            this.stopTokenSource?.Dispose();

            while (this.IsRunning)
            {
                Thread.Sleep(50);
            }
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        this.logger?.LogDebug("Disposing");
        // TODO ensure this informs the server, closes the websocket and completes all internal queues and ensures no leftover internal task is running !!!

        //if (this.IsRunning)
        //{
        //    await this.DisconnectAsync(CancellationToken.None);

        //    if (this.runningTask != null)
        //        await runningTask;
        //}

        var timer = this.refreshTokenTimer;
        this.refreshTokenTimer = null;
        if (timer != null)
        {
            await timer.DisposeAsync().ConfigureAwait(false);
        }

        this.outgoingQueue.Writer.TryComplete();
        this.incomingQueue.Writer.TryComplete();

        this.stopTokenSource?.Dispose();

        while (this.IsRunning)
        {
            await Task.Delay(50).ConfigureAwait(false);
        }
    }
    #endregion
}
