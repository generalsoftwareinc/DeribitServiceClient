namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task IsDeribitAvailableAsync(CancellationToken cancellationToken);
    Task RunAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
