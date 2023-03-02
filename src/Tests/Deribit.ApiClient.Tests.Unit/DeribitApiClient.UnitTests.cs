using Castle.Core.Logging;
using Deribit.ApiClient.Abstractions;
using Deribit.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deribit.ApiClient.Tests.Unit
{
    [TestClass]
    public class DeribitApiClient_UnitTests
    {
        [TestMethod]
        [TestCategory("InputValidation")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeribitApiClient_Constructor_ThrowsIf_OptionsIsNull()
        {
            // Arrange
            ILogger<DeribitApiClient> logger = Substitute.For<ILogger<DeribitApiClient>>();
            
            // Act
            var _ = new DeribitApiClient(null!, logger);
        }

        [TestMethod]
        [TestCategory("InputValidation")]
        public void DeribitApiClient_Constructor_DoesNotThrowIf_LoggerIsNull()
        {
            // Arrange
            IOptions<DeribitOptions> options = new OptionsWrapper<DeribitOptions>(Utils.GetValidDeribitOptions());

            // Act
            var apiClient = new DeribitApiClient(options, null);
            
            // Assert
            Assert.IsFalse(apiClient.IsRunning);
        }
    }
}