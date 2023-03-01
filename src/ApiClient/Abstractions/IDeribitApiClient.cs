namespace Deribit.ApiClient.Abstractions;

public interface IDeribitApiClient
{
    long TickerMessagesCount { get; }
    long BookMessagesCount { get; }

    Task RunAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
