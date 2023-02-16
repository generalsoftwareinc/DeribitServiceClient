namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task<bool> IsDeribitAvailableAsync(CancellationToken cancellationToken);
    Task<bool> RunAsync(CancellationToken cancellationToken);
    Task<bool> DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
