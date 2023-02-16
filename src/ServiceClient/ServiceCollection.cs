using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceClient.Abstractions;
using ServiceClient.Implements;
using ServiceClient.Implements.SocketClient;
using ServiceClient.Implements.SocketClient.DTOs;

namespace ServiceClient;

public static class ServiceCollection
{
    public static void AddServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDeribitClient, DeribitSocketClient>();
        services.AddTransient<IServiceClient, DeribitServiceClient>();
        services.AddOptions<DeribitOptions>()
        .Bind(configuration.GetSection(nameof(DeribitOptions)))
        .Validate(config =>
        {
            if (string.IsNullOrEmpty(config.ClientId) || string.IsNullOrEmpty(config.ClientSecret))
                return false;
            return true;
        });
    }
}