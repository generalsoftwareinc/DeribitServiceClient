using Deribit.ApiClient.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deribit.ApiClient.Tests.Unit
{
    internal static class Utils
    {
        internal static DeribitOptions GetValidDeribitOptions()
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

        internal static Dictionary<string, string> BuildMemoryConfigurationDataFrom(DeribitOptions options, string configSectionName = "DeribitOptions")
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
