using ConsoleApp.DTOs;
using ConsoleApp.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceClient;
using Spectre.Console;

#region Configuration
// Build configurations of appsettings.json and environment
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
#endregion

// Build a host with initialization of services of dependency injection 
#region Services
var host = Host.CreateDefaultBuilder()
    .UseConsoleLifetime()
    .ConfigureLogging((_, logging) =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options => options.SingleLine= true);
    })
    .ConfigureServices(services => {
        services.AddOptions<OutputOptions>()
                .Bind(config.GetSection(nameof(OutputOptions)));
        services.AddLogging();
        services.AddServiceClient(config);
        services.AddSingleton(new Table().Centered());
        services.AddTransient(sp => AnsiConsole.Live(sp.GetRequiredService<Table>()));
        services.AddTransient<SpectreOutputPipeline>();
        services.AddTransient<LogOutputPipeline>();
        services.AddTransient<Pipeline>((sp) => {
            var output = sp.GetRequiredService<IOptions<OutputOptions>>();
            return output.Value.Type switch
            {
                OutputOptions.OutputTypes.Console => sp.GetRequiredService<SpectreOutputPipeline>(),
                OutputOptions.OutputTypes.Logging => sp.GetRequiredService<LogOutputPipeline>(),
                _ => throw new NotSupportedException()
            };
        });
    })
    .Build();
#endregion
var logger = host.Services.GetRequiredService<ILogger<Program>>();
// Create a command app of Spectre Console
var cancellationToken = new CancellationTokenSource();

var pipeline = host.Services.GetRequiredService<Pipeline>();

async void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
{
    try
    {
        await pipeline.DisconnectAsync(cancellationToken.Token).ConfigureAwait(false);
        cancellationToken.Cancel();
        Console.CancelKeyPress -= Console_CancelKeyPress;
    }
    catch (Exception ex)  {
        logger.LogError($"Error desconecting {ex.Message}");
    }
    finally
    {
        e.Cancel = true;
    }
}

Console.CancelKeyPress += Console_CancelKeyPress;

await pipeline.RunAsync(cancellationToken.Token);
