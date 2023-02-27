using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using ServiceClient.Implements.DTOs;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;

namespace ServiceClient.Implements;

internal class DeribitServiceClient : IServiceClient
{
    private const string EndMessage = "END";
    private static int nextId = 0;
    private readonly DeribitOptions deribitOptions;
    private BookData? lastBook;
    private Timer? refreshTokenTimer;
    static protected readonly BlockingCollection<string> readMessageQueue = new();
    static protected readonly BlockingCollection<string> sendMessageQueue = new();
    public AuthResult? Credentials { get; protected set; }

    public DeribitServiceClient(IOptions<DeribitOptions> options)
    {
        deribitOptions = options.Value;
    }

    public event TickerReceivedEventHandler? OnTickerReceived;
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        sendMessageQueue.Add(EndMessage);
        return Task.CompletedTask;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        var socketWorker = Task.Run(() => DeribitSocketWorker(deribitOptions, cancellationToken), cancellationToken);
        sendMessageQueue.Add(CheckDeribitAvailable(), cancellationToken);
        sendMessageQueue.Add(Authenticate(deribitOptions), cancellationToken);
        sendMessageQueue.Add(Subscribe(deribitOptions), cancellationToken);
        sendMessageQueue.Add(SetHeartBeat(), cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var message = readMessageQueue.Take(cancellationToken);
            ParseMessage(message);
        }

        return socketWorker;
    }

    private void ParseMessage(string message) { 
        var isBookMessage = message.Contains(BookChannel, StringComparison.CurrentCulture);
        var isTikerMessage = message.Contains(TickerChannel, StringComparison.CurrentCulture);
        if (message.Contains("\"result\":[\"") && message.TryDeserialize<ActionResponse<string[]>>(out var subResponse))
        {
            ParseChannelSubscriptionResponse(subResponse!.Result);
        }
        else if (isBookMessage && message.TryDeserialize<BookResponse>(out var book))
        {
            lastBook = book!.Parameters?.Data;
        }
        else if (isTikerMessage && message.TryDeserialize<TickerResponse>(out var ticker))
        {
            OnTickerReceived?.Invoke(this, new TickerReceivedEventArgs(ticker!.Parameters?.Data, lastBook));
        }
        else if (message.Contains("test_request"))
        {
            sendMessageQueue.Add(BuildMessage("public/test", new object()));
        }
        else if (message.Contains("refresh_token"))
        {
            ParseCredentials(message);
        }
    }

    private static string CheckDeribitAvailable()
    {
        return BuildMessage("public/test", new object());
    }

    public static string Authenticate(DeribitOptions deribitOptions)
    {
        var data = new
        {
            grant_type = "client_credentials",
            client_id = deribitOptions.ClientId,
            client_secret = deribitOptions.ClientSecret,
        };
        return BuildMessage("public/auth", data);
    }

    private static string Subscribe(DeribitOptions deribitOptions)
    {
        var data = new
        {
            channels = new[] {
                $"book.{deribitOptions.InstrumentName}.{deribitOptions.BookInterval}",
                $"ticker.{deribitOptions.InstrumentName}.{deribitOptions.TickerInterval}"
}
        };
        return BuildMessage("private/subscribe", data);
    }

    private static string SetHeartBeat()
    {
        return BuildMessage("private/set_heartbeat", new object());
    }
    protected string BookChannel => $"book.{deribitOptions.InstrumentName}.{deribitOptions.BookInterval}";
    protected string TickerChannel => $"ticker.{deribitOptions.InstrumentName}.{deribitOptions.TickerInterval}";

    private void ParseChannelSubscriptionResponse(string[]? channels) { 

        if (channels == null)
            throw new NotSupportedException("Can't subscribe to the channels");
        var _channels = new[] { BookChannel, TickerChannel };
        var notSubscribedChannels = _channels
            .Except(channels)
            .ToArray();

        if (notSubscribedChannels.Any())
        {
            var channelsString = string.Join(", ", notSubscribedChannels);
            throw new NotSupportedException($"Can't subscribe to the following channels: {channelsString}");
        }
    }
    private void ParseCredentials(string message)
    {
        refreshTokenTimer?.Dispose();
        message.TryDeserialize<ActionResponse<AuthResult>>(out var credentials);
        Credentials = credentials?.Result;
        sendMessageQueue.Add(SetHeartBeat());

        if (Credentials?.ExpiresIn != null)
        {
            refreshTokenTimer = new Timer(Timer_Elapsed);
            var runAt = Credentials!.ExpiresIn! - 300;
            refreshTokenTimer.Change(runAt, runAt);
        }
    }

    private void Timer_Elapsed(object? state)
    {
        refreshTokenTimer?.Dispose();
        var data = new
        {
            grant_type = "refresh_token",
            refresh_token = Credentials!.RefreshToken,
        };
        sendMessageQueue.Add(BuildMessage("public/auth", data));
    }
    public static string BuildMessage<T>(string method, T data)
    {
        var request = new Request<T>
        {
            JsonRpcVersion = "2.0",
            Id = ++nextId,
            Method = method,
            Parameters = data,
        };
       return JsonSerializer.Serialize(request, jsonOptions);
    }

    static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static async Task DeribitSocketWorker(DeribitOptions deribitOptions, CancellationToken token)
    {
        using var ws = new WebsocketClient(new Uri(deribitOptions.WebSocketUrl));
        ws.MessageReceived.Subscribe(info => readMessageQueue.Add(info.Text));
        ws.ReconnectionHappened.Subscribe(info => { 
            if (info.Type != ReconnectionType.Initial)
            {
                sendMessageQueue.Add(Authenticate(deribitOptions));
                sendMessageQueue.Add(Subscribe(deribitOptions));
                sendMessageQueue.Add(SetHeartBeat());
            }
        });
        await ws.Start();
        while(!token.IsCancellationRequested) 
        {
            var message = sendMessageQueue.Take(token);
            if (message == EndMessage)
            {
                await ws.Stop(WebSocketCloseStatus.NormalClosure, message);
                break;
            }
            await ws.SendInstant(message);
        }
    }
}
