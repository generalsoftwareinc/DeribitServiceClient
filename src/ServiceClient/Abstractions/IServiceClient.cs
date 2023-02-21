namespace ServiceClient.Abstractions;

public interface IServiceClient
{
    Task RunAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    event TickerReceivedEventHandler OnTickerReceived;
}
