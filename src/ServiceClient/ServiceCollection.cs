using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceClient.Abstractions;
using ServiceClient.DTOs;
using ServiceClient.Implements;

namespace ServiceClient
{
    public static class ServiceCollection
    {
        public static void AddServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ISocketDataTransfer, SocketDataTransfer>();
            services.AddTransient<IDeribitSocketConnection, DeribitSocketConnection>();
            services.AddTransient<IServiceClient, DeribitServiceClient>();
            services.AddOptions<DeribitOptions>()
            .Bind(configuration.GetSection(nameof(DeribitOptions)))
            .Validate(config =>
            {
                if (string.IsNullOrEmpty(config.ApiKey) || string.IsNullOrEmpty(config.ApiSecret))
                    return false;
                return true;
            });
            services.AddOptions<InstrumentConfiguration>()
            .Bind(configuration.GetSection(nameof(InstrumentConfiguration)))
            .Validate(config =>
            {
                if (string.IsNullOrEmpty(config.InstrumentName) || string.IsNullOrEmpty(config.TickerInterval) || string.IsNullOrEmpty(config.BookInterval))
                    return false;
                return true;
            });
            services.AddOptions<WebSocketOptions>()
                .Bind(configuration.GetSection(nameof(WebSocketOptions)))
                .Validate(config =>
                {
                    if (string.IsNullOrEmpty(config.Url))
                        return false;
                    return true;
                });
        }
    }
}