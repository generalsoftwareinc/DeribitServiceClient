using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using ServiceClient.Implements.DTOs;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;
namespace ServiceClient.Implements;

internal class DeribitServiceClient : IServiceClient
{
    private int nextId = 0;
    private readonly DeribitOptions deribitOptions;
    private readonly ILogger<DeribitServiceClient> logger;
    private BookData? lastBook;
    private readonly WebsocketClient ws;
    private Timer? refreshTokenTimer;
    static protected readonly BlockingCollection<string> readMessageQueue = new();
    public AuthResult? Credentials { get; protected set; }

    public DeribitServiceClient(IOptions<DeribitOptions> options, ILogger<DeribitServiceClient> logger)
    {
        deribitOptions = options.Value;
        this.logger = logger;
        ws = new WebsocketClient(new Uri(deribitOptions.WebSocketUrl))
        {
            ReconnectTimeout = TimeSpan.FromMilliseconds(deribitOptions.ConnectionTimeoutInMilliseconds),
            IsReconnectionEnabled = true,
        };
        
    }

    public event TickerReceivedEventHandler? OnTickerReceived;
    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await ws.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await CheckDeribitAvailableAsync();
        await InitializeAsync();
        await SubscribeAsync();
        while (ws.IsRunning && !cancellationToken.IsCancellationRequested)
        {
            var message = readMessageQueue.Take(cancellationToken);
            ParseMessage(message);
        }
    }

    private static void OnMessage(ResponseMessage response)
    {
        var message = response.Text;
        if (string.IsNullOrEmpty(message)) return;
        readMessageQueue.Add(message);
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
            ws.Send(BuildMessage("public/test", new object()));
        }
        else if (message.Contains("refresh_token"))
        {
            ParseCredentials(message);
        }
    }

    public Task CheckDeribitAvailableAsync()
    {
        return ws.SendInstant(BuildMessage("public/test", new object()));
    }

    public async Task InitializeAsync()
    {
        var data = new
        {
            grant_type = "client_credentials",
            client_id = deribitOptions.ClientId,
            client_secret = deribitOptions.ClientSecret,
        };
        await ws.SendInstant(BuildMessage("public/auth", data));
        await ws.SendInstant(BuildMessage("private/set_heartbeat", data));
    }

    private async Task SubscribeAsync()
    {
        var data = new
        {
            channels = new[] { BookChannel, TickerChannel }
        };
        await ws.SendInstant(BuildMessage("private/subscribe", data));
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
        ws.Send(BuildMessage("public/auth", data));
    }
    public string BuildMessage<T>(string method, T data)
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

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        ws.MessageReceived.Subscribe(OnMessage);
        ws.DisconnectionHappened.Subscribe(info => logger.LogWarning("Disconnection: {t}, {d}", info.Type, info.Exception?.Message));
        ws.ReconnectionHappened.Subscribe(info => logger.LogWarning("Reconnection: {t}", info.Type));
        return ws.Start();
    }

    static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
