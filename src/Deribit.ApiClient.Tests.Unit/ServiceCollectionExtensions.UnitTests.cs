using Deribit.ApiClient.Abstractions;
using Deribit.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deribit.ApiClient.Tests.Unit
{
    [TestClass]
    public class ServiceCollectionExtensionsUnitTests
    {
        [TestMethod]
        public void AddDeribitApiClient_CanResolveDeribitOptions_FromDefaultConfigSection()
        {
            // Arrange
            var options = GetValidDeribitOptions();
            var myConfiguration = BuildConfigurationFrom(options);
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();
            var services = new ServiceCollection();

            // Act
            var serviceProvider = services
                .AddDeribitApiClient(configuration)
                .BuildServiceProvider();
            var resolvedOptions = serviceProvider.GetRequiredService<IOptions<DeribitOptions>>();

            // Assert
            Assert.IsNotNull(resolvedOptions);
            Assert.AreEqual(options, resolvedOptions.Value);
        }

        [TestMethod]
        public void AddDeribitApiClient_CanResolveDeribitOptions_FromCustomConfigSection()
        {
            // Arrange
            string customSectionName = "customSectionName";
            var options = GetValidDeribitOptions() with
            {
                HeartBeatInterval = 40,
                ClientId = customSectionName,
                ClientSecret = customSectionName
            };
            var myConfiguration = BuildConfigurationFrom(options, customSectionName);
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();
            var services = new ServiceCollection();

            // Act
            var serviceProvider = services
                .AddDeribitApiClient(configuration, customSectionName)
                .BuildServiceProvider();
            var resolvedOptions = serviceProvider.GetRequiredService<IOptions<DeribitOptions>>();

            // Assert
            Assert.IsNotNull(resolvedOptions);
            Assert.AreEqual(options, resolvedOptions.Value);
        }

        [TestMethod]
        public void AddDeribitApiClient_CanResolveDeribitApiClient()
        {
            // Arrange
            var options = GetValidDeribitOptions();
            var myConfiguration = BuildConfigurationFrom(options);
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();
            var services = new ServiceCollection();

            // Act
            var serviceProvider = services
                .AddLogging()
                .AddDeribitApiClient(configuration)
                .BuildServiceProvider();
            var apiClient = serviceProvider.GetRequiredService<IDeribitApiClient>();

            // Assert
            Assert.IsNotNull(apiClient);
            Assert.IsFalse(apiClient.IsRunning);
        }

        private static DeribitOptions GetValidDeribitOptions()
        {
            return new DeribitOptions()
            {
                BookInterval = DeribitOptions.ValidSubscriptionIntervalValues[0],
                ClientId = "unit.test",
                ClientSecret = "123",
                HeartBeatInterval = 30,
                InstrumentName = "BTC-PERPETUAL",
                TickerInterval = DeribitOptions.ValidSubscriptionIntervalValues[0],
                WebSocketUrl = "wss://test.deribit.com/ws/api/v2",
            };
        }

        private static Dictionary<string, string> BuildConfigurationFrom(DeribitOptions options, string configSectionName = "DeribitOptions")
        {
            return new Dictionary<string, string>
                {
                    { $"{configSectionName}:{nameof(options.ClientId)}", options.ClientId },
                    { $"{configSectionName}:{nameof(options.ClientSecret)}", options.ClientSecret },
                    { $"{configSectionName}:{nameof(options.WebSocketUrl)}", options.WebSocketUrl },
                    { $"{configSectionName}:{nameof(options.InstrumentName)}", options.InstrumentName },
                    { $"{configSectionName}:{nameof(options.TickerInterval)}", options.TickerInterval },
                    { $"{configSectionName}:{nameof(options.BookInterval)}", options.BookInterval },
                    { $"{configSectionName}:{nameof(options.HeartBeatInterval)}", options.HeartBeatInterval.ToString() },
                };
        }
    }
}