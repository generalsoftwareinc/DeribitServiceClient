namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task<bool> IsDeribitAvailableAsync(CancellationToken cancellationToken);
    Task<bool> InitializeAsync(CancellationToken cancellationToken);
    Task<bool> Authenticate(CancellationToken cancellationToken);
    Task<bool> SubscribeToChannelsAsync(CancellationToken cancellationToken);
    Task ReceiveDataFromChannelsAsync(CancellationToken cancellationToken);
    Task<bool> DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
