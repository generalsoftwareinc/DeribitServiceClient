namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task<bool> IsDeribitAvailableAsync();
    Task<bool> InitializeAsync();
    Task<bool> DisconnectAsync();

    event EventHandler OnTickerReceived;
}
