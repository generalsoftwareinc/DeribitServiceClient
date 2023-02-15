using ServiceClient.Abstractions;
using ServiceClient.DTOs;

namespace ConsoleApp;

internal class Pipeline
{
    private readonly IServiceClient client;

    public Pipeline(IServiceClient client)
    {
        this.client = client;
    }

    public async Task RunAsync()
    {
        //TODO: call with cancellation token
        await client.IsDeribitAvailableAsync(CancellationToken.None);
        await client.InitializeAsync(CancellationToken.None);
        await client.DisconnectAsync(CancellationToken.None);

        client.OnTickerReceived += Client_OnTickerReceived;
    }

    private void Client_OnTickerReceived(object? sender, EventArgs e)
    {
        if (sender is UpcommingTickerUpdate update)
        {
            // TODO
        }
    }
}
