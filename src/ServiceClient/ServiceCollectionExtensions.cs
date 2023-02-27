using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Deribit.ServiceClient.Serialization;
using Deribit.ServiceClient.Configuration;
using Deribit.ServiceClient.Abstractions;

namespace Deribit.ServiceClient;

public static class ServiceCollectionExtensions
{
    public static void AddDeribitApiClient(this IServiceCollection services, IConfiguration configuration, string deribitOptionsConfigSectionName = nameof(DeribitOptions))
    {
        services.AddTransient<IDeribitApiClient, DeribitApiClient>();
        services.AddOptions<DeribitOptions>()
            .Bind(configuration.GetSection(deribitOptionsConfigSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}