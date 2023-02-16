using ServiceClient;
using ServiceClient.Abstractions;

namespace ConsoleApp.Pipelines;

internal abstract class Pipeline
{
    private readonly IServiceClient client;

    protected Pipeline(IServiceClient client)
    {
        this.client = client;
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        WritePipelineStep("Checking Deribit API Availability");
        await client.IsDeribitAvailableAsync(cancellationToken);

        PreInitializeHook();
        client.OnTickerReceived += Client_OnTickerReceived;
        await client.InitializeAsync(cancellationToken);

        var authenticated = await client.Authenticate(cancellationToken);

        if (!authenticated)
            throw new Exception("Error trying to authetincate!");

        await client.SubscribeToChannelsAsync(cancellationToken);

        await client.ReceiveDataFromChannelsAsync(cancellationToken);

        WritePipelineStep("Disconnecting");
        await client.DisconnectAsync(cancellationToken);
    }

    protected abstract void Client_OnTickerReceived(object? sender, TickerReceivedEventArgs e);

    protected abstract void WritePipelineStep(string stepInfo);

    protected virtual void PreInitializeHook()
    {
        WritePipelineStep("Initilize connection to Deribit API Web Socket");
    }
}
