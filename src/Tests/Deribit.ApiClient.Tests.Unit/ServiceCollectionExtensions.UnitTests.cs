using Deribit.ApiClient.Abstractions;
using Deribit.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deribit.ApiClient.Tests.Unit
{
    [TestClass]
    public class ServiceCollectionExtensions_UnitTests
    {
        [TestMethod]
        [TestCategory("Logic")]
        public void AddDeribitApiClient_CanResolveDeribitOptions_FromDefaultConfigSection()
        {
            // Arrange
            var options = Utils.GetValidDeribitOptions();
            var myConfiguration = Utils.BuildMemoryConfigurationDataFrom(options);
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
        [TestCategory("Logic")]
        public void AddDeribitApiClient_CanResolveDeribitOptions_FromCustomConfigSection()
        {
            // Arrange
            string customSectionName = "customSectionName";
            var options = Utils.GetValidDeribitOptions() with
            {
                HeartBeatInterval = 40,
                ClientId = customSectionName,
                ClientSecret = customSectionName
            };
            var myConfiguration = Utils.BuildMemoryConfigurationDataFrom(options, customSectionName);
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
        [TestCategory("Logic")]
        public void AddDeribitApiClient_CanResolveDeribitApiClient()
        {
            // Arrange
            var options = Utils.GetValidDeribitOptions();
            var myConfiguration = Utils.BuildMemoryConfigurationDataFrom(options);
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
    }
}