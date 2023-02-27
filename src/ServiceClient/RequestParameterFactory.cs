using Deribit.ServiceClient.Configuration;
using Deribit.ServiceClient.DTOs.Auth;
using Deribit.ServiceClient.DTOs.Subscribe;
using Deribit.ServiceClient.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Deribit.ServiceClient
{
    internal class RequestParameterFactory
    {
        internal SubscribeRequest GetSubscribeRequest(DeribitOptions deribitOptions)
        {
            return new SubscribeRequest()
            {
                Channels = new[]
                {
                    $"book.{deribitOptions.InstrumentName}.{deribitOptions.BookInterval}",
                    $"ticker.{deribitOptions.InstrumentName}.{deribitOptions.TickerInterval}"
                }
            };
        }

        internal AuthRequest GetAuthenticateByClientCredentialsRequest(DeribitOptions deribitOptions)
        {
            return new AuthRequest()
            {
                GrantType = "client_credentials",
                ClientId = deribitOptions.ClientId,
                ClientSecret = deribitOptions.ClientSecret,
            };
        }

        internal AuthRequest GetAuthenticateByRefreshTokenRequest(string refreshToken)
        {
            return new AuthRequest()
            {
                GrantType = "refresh_token",
                RefreshToken = refreshToken,
            };
        }
    }
}
