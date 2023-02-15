using ConsoleApp;
using ConsoleApp.Commands;
using ConsoleApp.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceClient;
using ServiceClient.DTOs;
using Spectre.Console.Cli;

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
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddServiceClient(config);
        services.AddTransient<Pipeline>();
        services.AddTransient<AsyncLogCommand>();
        services.AddTransient<AsyncSpectreCommand>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    })
    .Build();
#endregion

// Create a command app of Spectre Console
var app = new CommandApp(host.Services.GetRequiredService<ITypeRegistrar>());

app.Configure(config => {
    config.AddBranch("output", c =>
    {
        c.AddCommand<AsyncLogCommand>("log");
        c.AddCommand<AsyncSpectreCommand>("spectre");
    });
});

await app.RunAsync(args);