namespace Deribit.ApiClient.Abstractions;

public interface IDeribitApiClient
{
    long TickerMessagesCount { get; }
    long BookMessagesCount { get; }
    long SubscriptionMessagesCount { get; }
    long HeartBeatMessagesCount { get; }
    long TokenRefreshMessagesCount { get; }

    /// <summary>
    /// Gets a boolean indicating whether the API client is running, or not.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts the API client by connecting to the server, authenticating, then process incoming and outgoing messages.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
    Task RunAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Initiates disconnecting from the server by closing the websocket, leaving any pending incoming or outgoing messages in the internal queues.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
    Task DisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resets internal state, clearing all possibly leftover messages in internal queues to ensure the next call to <see cref="RunAsync(CancellationToken)"/> won't start processing leftover messages from the previous run.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
    void Reset();

    /// <summary>
    /// Event that gets triggered when new ticker data received.
    /// </summary>
    event TickerReceivedEventHandler OnTickerReceived;
}
