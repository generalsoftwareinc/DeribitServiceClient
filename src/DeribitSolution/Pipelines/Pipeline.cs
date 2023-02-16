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
        await client.RunAsync(cancellationToken);

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
