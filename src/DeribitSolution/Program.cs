using ConsoleApp.TestClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    .AddUserSecrets("c252577f-cf0f-45f3-99b3-4243ba11d930")
    .Build();
logger.LogInformation("Injected appsettings.json file to project");
logger.LogInformation("Injected secured settings to project");
Console.WriteLine();
serviceCollection.AddSingleton<IConfiguration>(configuration);
logger.LogInformation("Injected configuration");
Console.WriteLine();
serviceCollection.AddSingleton<TestClass>();
var apiKey = configuration.GetSection("DeribitAPI").Get<ServiceClient.ApiKey>();
if (apiKey != null)
    serviceCollection.AddSingleton(apiKey);
var serviceProvider = serviceCollection.BuildServiceProvider();
logger.LogInformation("Created Service provider");
Console.WriteLine();
logger.LogInformation("Getting object to test dependency injection");
Console.WriteLine();
var test = serviceProvider.GetService<TestClass>();
test?.TestMethod();
var apiKey2 = serviceProvider.GetService<ServiceClient.ApiKey>();
logger.LogInformation("Depdency injection configured succesfully");
Console.WriteLine();
