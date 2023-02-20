using ServiceClient;
using ServiceClient.Abstractions;
using ServiceClient.Exceptions;

namespace ConsoleApp.Pipelines;

internal abstract class Pipeline
{
    private readonly IServiceClient client;

    protected Pipeline(IServiceClient client)
    {
        this.client = client;
    }
    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        WritePipelineStep("Disconnecting");
        try
        {
            await client.DisconnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            WritePipelineStep(ex.Message);
        }
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        WritePipelineStep("Checking Deribit API Availability");
        try
        {
            await client.IsDeribitAvailableAsync(cancellationToken);
            PreInitializeHook();
            client.OnTickerReceived += Client_OnTickerReceived;
            await client.RunAsync(cancellationToken);
        }
        catch (UnavailableDeribitException ex)
        {
            WritePipelineStep(ex.Message);
        }
        catch (NotSupportedException ex)
        {
            WritePipelineStep(ex.Message);
        }
        catch
        {
            WritePipelineStep("Sorry, your configuration is not OK. Check and try again later.");
        }
        finally
        {
            await DisconnectAsync(cancellationToken);
        }
    }

    protected abstract void Client_OnTickerReceived(object? sender, TickerReceivedEventArgs e);

    protected abstract void WritePipelineStep(string stepInfo);

    protected virtual void PreInitializeHook()
    {
        WritePipelineStep("Initilize connection to Deribit API Web Socket");
    }
}
