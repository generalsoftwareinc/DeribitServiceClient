using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceClient.Abstractions;
using ServiceClient.Implements;
using ServiceClient.Implements.DTOs;

namespace ServiceClient;

public static class ServiceCollection
{
    public static void AddServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IServiceClient, DeribitServiceClient>();
        services.AddOptions<DeribitOptions>()
            .Bind(configuration.GetSection(nameof(DeribitOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}