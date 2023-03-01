using Deribit.ApiClient.Configuration;
using Deribit.ApiClient.DTOs.Auth;
using Deribit.ApiClient.DTOs.Subscribe;

namespace Deribit.ApiClient
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
