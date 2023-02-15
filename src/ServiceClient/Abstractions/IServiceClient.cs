namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task<bool> IsDeribitAvailableAsync(CancellationToken cancellationToken);
    Task<bool> InitializeAsync(CancellationToken cancellationToken);
    Task<bool> DisconnectAsync(CancellationToken cancellationToken);

    event EventHandler OnTickerReceived;
}
