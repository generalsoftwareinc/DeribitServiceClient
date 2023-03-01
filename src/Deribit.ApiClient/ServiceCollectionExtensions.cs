using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Deribit.ApiClient.Configuration;
using Deribit.ApiClient.Abstractions;

namespace Deribit.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeribitApiClient(this IServiceCollection services, IConfiguration configuration, string deribitOptionsConfigSectionName = nameof(DeribitOptions))
    {
        services.AddTransient<IDeribitApiClient, DeribitApiClient>();
        services.AddOptions<DeribitOptions>()
            .Bind(configuration.GetSection(deribitOptionsConfigSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}