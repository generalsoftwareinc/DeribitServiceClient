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
        await client.IsDeribitAvailableAsync();
        await client.InitializeAsync();
        await client.DisconnectAsync();

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
