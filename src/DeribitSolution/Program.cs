using ConsoleApp.Commands;
using ConsoleApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceClient;
using Spectre.Console.Cli;

var host = Host.CreateDefaultBuilder()
    .UseConsoleLifetime()
    .ConfigureLogging((_, logging) =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options => options.SingleLine= true);
    })
    .ConfigureServices(services => {
        services.AddLogging();
        services.AddServiceClient();
        services.AddTransient<AsyncLogCommand>();
        services.AddTransient<AsyncSpectreCommand>();
        services.AddSingleton<ITypeRegistrar>(new TypeRegistrar(services));
    })
    .Build();

var app = new CommandApp(host.Services.GetRequiredService<ITypeRegistrar>());

app.Configure(config => {
    config.AddBranch("output", c =>
    {
        c.AddCommand<AsyncLogCommand>("log");
        c.AddCommand<AsyncSpectreCommand>("spectre");
    });
});

await app.RunAsync(args);