using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace ConsoleApp.Commands;

internal class AsyncLogCommand : AsyncCommand<AsyncLogCommand.Settings>
{
    private readonly ILogger<AsyncLogCommand> _logger;
    public AsyncLogCommand(ILogger<AsyncLogCommand> logger)
    {
        _logger = logger;
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.LogInformation("Starting");
        return Task.FromResult(0);
    }

    public sealed class Settings : CommandSettings
    {
      
    }
}
