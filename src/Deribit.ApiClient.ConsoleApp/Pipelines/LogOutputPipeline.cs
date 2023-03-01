using Microsoft.Extensions.Logging;
using Deribit.ApiClient.Abstractions;
using Deribit.ApiClient;

namespace ConsoleApp.Pipelines;

internal class LogOutputPipeline : Pipeline
{
    private readonly ILogger<Pipeline> logger;
    public LogOutputPipeline(IDeribitApiClient client, ILogger<Pipeline> logger) : base(client)
    {
        this.logger = logger;
    }

    protected override void Client_OnTickerReceived(object? sender, TickerReceivedEventArgs e)
    {
        logger.LogInformation("Ticker: {Ticker}', Book: {Book}", e.Ticker, e.Book);
    }

    protected override void WritePipelineStep(string stepInfo)
    {
        logger.LogInformation("Step: {stepInfo}", stepInfo);
    }
}
