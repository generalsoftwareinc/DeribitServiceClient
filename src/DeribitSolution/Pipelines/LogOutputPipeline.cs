using Microsoft.Extensions.Logging;
using ServiceClient.Abstractions;

namespace ConsoleApp.Pipelines;

internal class LogOutputPipeline : Pipeline
{
    private readonly ILogger<Pipeline> logger;
    public LogOutputPipeline(IServiceClient client, ILogger<Pipeline> logger) : base(client)
    {
        this.logger = logger;
    }

    protected override void Client_OnTickerReceived(object? sender, EventArgs e)
    {
        logger.LogInformation("Capture a new update {@EventArgs}", e);
    }

    protected override void WritePipelineStep(string stepInfo)
    {
        logger.LogInformation("Step: {stepInfo}", stepInfo);
    }
}
