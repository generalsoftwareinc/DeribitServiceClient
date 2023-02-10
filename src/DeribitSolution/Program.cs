using ConsoleApp.TestClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Authentication.ExtendedProtection;

var serviceCollection = new ServiceCollection();
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("System", LogLevel.Warning)
        .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
        .AddConsole();
});
ILogger logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Created log feature");
Console.WriteLine();
IConfiguration configuration;
var path = Directory.GetCurrentDirectory();
configuration = new ConfigurationBuilder()
    .SetBasePath(path)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
logger.LogInformation("Injected appsettings.json file to project");
Console.WriteLine();
serviceCollection.AddSingleton<IConfiguration>(configuration);
logger.LogInformation("Injected configuration");
Console.WriteLine();
serviceCollection.AddSingleton<TestClass>();
var serviceProvider = serviceCollection.BuildServiceProvider();
logger.LogInformation("Created Service provider");
Console.WriteLine();
logger.LogInformation("Getting object to test dependency injection");
Console.WriteLine();
var test = serviceProvider.GetService<TestClass>();
test.TestMethod();
logger.LogInformation("Depdency injection configured succesfully");
Console.WriteLine();