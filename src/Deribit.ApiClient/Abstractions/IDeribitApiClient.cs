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

    Task RunAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
