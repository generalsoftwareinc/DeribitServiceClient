using Microsoft.Extensions.DependencyInjection;
using ServiceClient.Abstractions;
using ServiceClient.Implements;

namespace ServiceClient
{
    public static class ServiceCollection
    {
        public static void AddServiceClient(this IServiceCollection services)
        {
            services.AddTransient<IServiceClient, MockServiceClient>();
        }
    }
}