using Deribit.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deribit.ApiClient.Tests.Integration
{
    internal static class Utils
    {
        internal static readonly Lazy<IOptions<DeribitOptions>> DeribitOptionsFromAppConfig = new(GetValidDeribitOptionsFromConfig, true);

        public static readonly TestDebugLoggerFactory LoggerFactory = new TestDebugLoggerFactory();

        private static IOptions<DeribitOptions> GetValidDeribitOptionsFromConfig()
        {
            return GetValidDeribitOptionsFromConfig(nameof(DeribitOptions));
        }

        private static IOptions<DeribitOptions> GetValidDeribitOptionsFromConfig(string configSectionName)
        {
            if (configSectionName == null)
                throw new ArgumentNullException(nameof(configSectionName));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            var options = new DeribitOptions();
            configuration.Bind(configSectionName, options);
            var validationContext = new ValidationContext(options);

            Validator.ValidateObject(options, validationContext, true);

            return new OptionsWrapper<DeribitOptions>(options);
        }

        internal static DeribitApiClient GetDeribitApiClient(DeribitOptions? deribitOptions = null, ILogger<DeribitApiClient>? logger = null)
        {
            var options = deribitOptions == null
                ? DeribitOptionsFromAppConfig.Value
                : new OptionsWrapper<DeribitOptions>(deribitOptions);

            return new DeribitApiClient(options, logger);
        }
    }
}
