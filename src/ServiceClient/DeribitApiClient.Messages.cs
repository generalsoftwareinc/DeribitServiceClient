using Deribit.ServiceClient.Abstractions;
using Deribit.ServiceClient.Configuration;
using Deribit.ServiceClient.DTOs;
using Deribit.ServiceClient.DTOs.Auth;
using Deribit.ServiceClient.DTOs.Book;
using Deribit.ServiceClient.DTOs.Subscribe;
using Deribit.ServiceClient.DTOs.Ticker;
using Deribit.ServiceClient.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;

namespace Deribit.ServiceClient;

internal partial class DeribitApiClient
{
    protected string BookChannel => $"book.{options.InstrumentName}.{options.BookInterval}";
    protected string TickerChannel => $"ticker.{options.InstrumentName}.{options.TickerInterval}";

    private static readonly object EmptyObject = new();

    private string GetTestRequestMessage()
    {
        return JsonStringMessageBuilder.BuildMessage(++nextId, "public/test", EmptyObject);
    }

    private string GetAuthenticateByRefreshTokenMessage(string refreshToken)
    {
        var data = this.requestParameterFactory.GetAuthenticateByRefreshTokenRequest(refreshToken);

        return JsonStringMessageBuilder.BuildMessage(++nextId, "public/auth", data);
    }

    private string GetAuthenticateByClientCredentialsMessage(DeribitOptions deribitOptions)
    {
        var data = this.requestParameterFactory.GetAuthenticateByClientCredentialsRequest(deribitOptions);

        return JsonStringMessageBuilder.BuildMessage(++nextId, "public/auth", data);
    }

    private string GetSubscribeMessage(DeribitOptions deribitOptions)
    {
        var data = this.requestParameterFactory.GetSubscribeRequest(deribitOptions);

        return JsonStringMessageBuilder.BuildMessage(++nextId, "private/subscribe", data);
    }

    private string GetSetHeartBeatMessage()
    {
        return JsonStringMessageBuilder.BuildMessage(++nextId, "private/set_heartbeat", EmptyObject);
    }

    private void ParseMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        var isBookMessage = message.Contains(BookChannel, StringComparison.InvariantCulture);
        var isTikerMessage = !isBookMessage && message.Contains(TickerChannel, StringComparison.InvariantCulture);

        // if it is a subscription response
        if (!isBookMessage && !isTikerMessage && message.Contains("\"result\":[\"") && message.TryDeserialize<ActionResponse<string[]>>(out var subResponse))
        {
            HandleChannelSubscriptionResponse(subResponse!.Result);
        }
        // if it is a book message
        else if (isBookMessage && message.TryDeserialize<BookResponse>(out var book))
        {
            BookMessagesCount++;
            lastBook = book!.Parameters?.Data;
        }
        // if it is a ticker message
        else if (isTikerMessage && message.TryDeserialize<TickerResponse>(out var ticker))
        {
            TickerMessagesCount++;
            OnTickerReceived?.Invoke(this, new TickerReceivedEventArgs(ticker!.Parameters?.Data, lastBook));
        }
        // if the server asked for a heartbeat call
        else if (message.Contains("test_request"))
        {
            sendMessageQueue.Add(GetTestRequestMessage());
        }
        // if it is an auth response
        else if (message.Contains("refresh_token"))
        {
            HandleAuthResponse(message);
        }
    }

    private void HandleChannelSubscriptionResponse(string[]? channels)
    {

        if (channels == null)
            throw new NotSupportedException("Can't subscribe to the channels");
        var _channels = new[] { BookChannel, TickerChannel };
        var notSubscribedChannels = _channels
            .Except(channels, StringComparer.InvariantCulture)
            .ToArray();

        if (notSubscribedChannels.Any())
        {
            var channelsString = string.Join(", ", notSubscribedChannels);
            throw new NotSupportedException($"Can't subscribe to the following channels: {channelsString}");
        }
    }
    private void HandleAuthResponse(string message)
    {
        refreshTokenTimer?.Dispose();

        // if we could not parse the credentials
        if (!message.TryDeserialize<ActionResponse<AuthResult>>(out var credentials) || credentials?.Result == null)
        {
            throw new ArgumentException("Could not parse credentials.");
        }

        Credentials = credentials.Result;

        sendMessageQueue.Add(GetSetHeartBeatMessage());

        var runAtSec = Credentials!.ExpiresIn! - 300; // run 5 minutes before the expiration

        //TimeSpan expiration = TimeSpan.FromSeconds(Credentials.ExpiresIn);
        //TimeSpan runAt = expiration.Subtract(TimeSpan.FromMinutes(5));

        refreshTokenTimer = new Timer(RefreshTokenTimerTicked);
        refreshTokenTimer.Change(runAtSec, runAtSec);
    }
}
