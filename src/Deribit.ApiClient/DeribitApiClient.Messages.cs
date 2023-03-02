using Deribit.ApiClient.Configuration;
using Deribit.ApiClient.DTOs;
using Deribit.ApiClient.DTOs.Auth;
using Deribit.ApiClient.DTOs.Book;
using Deribit.ApiClient.DTOs.Ticker;
using Deribit.ApiClient.Serialization;

namespace Deribit.ApiClient;

partial class DeribitApiClient
{
    private static readonly object EmptyObject = new();

    protected string BookChannelName => $"book.{options.InstrumentName}.{options.BookInterval}";
    protected string TickerChannelName => $"ticker.{options.InstrumentName}.{options.TickerInterval}";

    public long TotalReceivedMessagesCount { get; private set; }
    public long TickerMessagesCount { get; private set; }
    public long BookMessagesCount { get; private set; }
    public long SubscriptionMessagesCount { get; private set; }
    public long HeartBeatMessagesCount { get; private set; }
    public long TokenRefreshMessagesCount { get; private set; }

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
        if (isDisposingOrDisposed || string.IsNullOrEmpty(message))
            return;

        var isBookMessage = message.Contains(BookChannelName, StringComparison.InvariantCulture);
        var isTikerMessage = !isBookMessage && message.Contains(TickerChannelName, StringComparison.InvariantCulture);

        // if it is a subscription response
        if (!isBookMessage && !isTikerMessage && message.Contains("\"result\":[\"") && message.TryDeserialize<ActionResponse<string[]>>(out var subResponse))
        {
            SubscriptionMessagesCount++;
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
            HeartBeatMessagesCount++;
            EnqueueOutgoingMessage(GetTestRequestMessage(), CancellationToken.None);
        }
        // if it is an auth response
        else if (message.Contains("refresh_token"))
        {
            TokenRefreshMessagesCount++;
            HandleAuthResponse(message);
        }
    }

    private void HandleChannelSubscriptionResponse(string[]? channels)
    {

        if (channels == null)
            throw new NotSupportedException("Can't subscribe to the channels");
        var _channels = new[] { BookChannelName, TickerChannelName };
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
        DisposeRefreshTokenTimer();

        // if we could not parse the credentials
        if (!message.TryDeserialize<ActionResponse<AuthResult>>(out var credentials) || credentials?.Result == null)
        {
            throw new ArgumentException("Could not parse credentials.");
        }

        Credentials = credentials.Result;

        EnqueueOutgoingMessage(GetSetHeartBeatMessage(), CancellationToken.None);

        var runAtSec = Credentials!.ExpiresIn! - 300; // run 5 minutes before the expiration

        //TimeSpan expiration = TimeSpan.FromSeconds(Credentials.ExpiresIn);
        //TimeSpan runAt = expiration.Subtract(TimeSpan.FromMinutes(5));

        StartNewRefreshTokenTimer(runAtSec);
    }
}
