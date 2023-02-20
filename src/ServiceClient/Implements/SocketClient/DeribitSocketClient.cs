using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using ServiceClient.Implements.SocketClient.DTOs;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceClient.Implements.SocketClient;

internal class DeribitSocketClient : IDeribitClient
{
    private int nextId = 0;
    private const int readBufferSize = 4096;
    private readonly ClientWebSocket webSocket;
    private readonly DeribitOptions deribitOptions;
    public event EventHandler<BookReadedEventArgs>? OnBookReaded; 
    public event EventHandler<TickerReadedEventArgs>? OnTickerReaded;
    private Timer? refreshTokenTimer;
    static protected readonly BlockingCollection<string> readMessageQueue = new();
    
    public AuthResult? Credentials { get; protected set; }

    public DeribitSocketClient(IOptions<DeribitOptions> options)
    {
        deribitOptions = options.Value;
        webSocket = new ClientWebSocket()
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromSeconds(deribitOptions.KeepAliveIntervalInSeconds),
            }
        };
    }
    protected string BookChannel => $"book.{deribitOptions.InstrumentName}.{deribitOptions.BookInterval}";
    protected string TickerChannel => $"ticker.{deribitOptions.InstrumentName}.{deribitOptions.TickerInterval}";

    private async void Timer_Elapsed(object? state)
    {
        refreshTokenTimer?.Dispose();
        await AuthenticateWithRefreshTokenAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private Task SendTestAsync(CancellationToken token)
    {
        return SendAsync("public/test", new object(), token);
    }

    private Task SendAsync<T>(string method, T data, CancellationToken cancellationToken)
    {
        if (webSocket.State != WebSocketState.Open) return Task.FromException(new NotSupportedException("The web socket is not open"));
        var request = new Request<T>
        {
            JsonRpcVersion = "2.0",
            Id = ++nextId,
            Method = method,
            Parameters = data,
        };
        var jsonMessage = JsonSerializer.Serialize(request, jsonOptions);
        var sendBuffer = new byte[jsonMessage.Length];

        Encoding.Default.GetBytes(jsonMessage, 0, jsonMessage.Length, sendBuffer, 0);

        return webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task ConnectAsync(CancellationToken token)
    {
        if (webSocket.State == WebSocketState.Open) return;
        await webSocket.ConnectAsync(new Uri(deribitOptions.WebSocketUrl), token);
    }

    private async Task<ActionResponse<T>?> ReadAsync<T>(CancellationToken token)
        where T : class
    {
        var message = await ReadOneMessageAsync(webSocket, token);
        message.TryDeserialize<ActionResponse<T>>(out var result);
        return result;
    }

    public async Task DisconnectAsync(CancellationToken token)
    {
        await DisableHeartbeatAsync(token);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        webSocket.Dispose();
    }

    public async Task CheckAvailabilityAsync(CancellationToken token)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            await ConnectAsync(token);
        }

        await SendTestAsync(token);
        var result = await ReadAsync<object>(token);
        if (result?.Result == null)
        {
            throw new NotSupportedException("Sorry, your configuration is not OK. Check and try again later.");
        }
    }
    public async Task InitializeAsync(CancellationToken token)
    {
        await AuthenticateAsync(token);
        await SetHeartbeatAsync(token);
    }

    private async Task AuthenticateAsync(CancellationToken token)
    {
        var data = new
        {
            grant_type = "client_credentials",
            client_id = deribitOptions.ClientId,
            client_secret = deribitOptions.ClientSecret,
        };
        await AuthenticateWithDataAsync(data, token);
        var credentials = await ReadOneMessageAsync(webSocket, token);
        ParseCredentials(credentials);
    }

    private Task AuthenticateWithRefreshTokenAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Credentials?.RefreshToken)) return Task.CompletedTask;
        var data = new
        {
            grant_type = "refresh_token",
            refresh_token = Credentials!.RefreshToken,
        };
        return AuthenticateWithDataAsync(data, token);
    }

    private async Task AuthenticateWithDataAsync(object data, CancellationToken token)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            await ConnectAsync(token);
        }
        await SendAsync("public/auth", data, token);
    }

    private Task SetHeartbeatAsync(CancellationToken token)
    {
        var data = new
        {
            interval = deribitOptions.HeartBeatInterval
        };
        return Task.WhenAll(SendAsync("public/set_heartbeat", data, token), ReadOneMessageAsync(webSocket, token));
    }

    private async Task DisableHeartbeatAsync(CancellationToken token)
    {
        await SendAsync("public/disable_heartbeat", new object(), token);
    }

    public async Task SubscribeAsync(CancellationToken token)
    {
        var data = new
        {
            channels = new[] { BookChannel, TickerChannel }
        };
        await SendAsync("private/subscribe", data, token);
        var channelsSubscribed = await ReadAsync<string[]>(token);

        if (channelsSubscribed?.Result == null)
            throw new NotSupportedException("Can't subscribe to the channels");

        var notSubscribedChannels = data.channels
            .Except(channelsSubscribed.Result)
            .ToArray();

        if (notSubscribedChannels.Any())
        {
            var channels = string.Join(", ", notSubscribedChannels);
            throw new NotSupportedException($"Can't subscribe to the following channels: {channels}");
        }
    }

    public async Task ContinueReadAsync(CancellationToken token)
    {
        _ = Task.Run(() => ReceiveMessageAsync(webSocket, token), token);
        while (!token.IsCancellationRequested)
        {
            var message = readMessageQueue.Take(token);
            var isBookMessage = message.Contains(BookChannel, StringComparison.CurrentCulture);
            var isTikerMessage = message.Contains(TickerChannel, StringComparison.CurrentCulture);

            if (isBookMessage && message.TryDeserialize<BookResponse>(out var book))
            {
                OnBookReaded?.Invoke(this, new BookReadedEventArgs(book!));
            }
            else if (isTikerMessage && message.TryDeserialize<TickerResponse>(out var ticker) )
            {
                OnTickerReaded?.Invoke(this, new TickerReadedEventArgs(ticker!));
            }
            else if (message.Contains("test_request"))
            {
                await SendTestAsync(token);
            }
            else if (message.Contains("refresh_token"))
            {
                ParseCredentials(message);
            }
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

    private static readonly StringBuilder jsonResult = new();
    private static readonly ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(readBufferSize, readBufferSize);

    async static Task ReceiveMessageAsync(ClientWebSocket ws, CancellationToken token)
    {
        while (ws.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            var message = await ReadOneMessageAsync(ws, token);
            if (message.Length > 0)
            {
                readMessageQueue.Add(message, token);
            }
        }
    }
    static async Task<string> ReadOneMessageAsync(ClientWebSocket ws, CancellationToken token)
    {
        WebSocketReceiveResult taskResult;

        jsonResult.Clear();
        do
        {
            taskResult = await ws.ReceiveAsync(buffer, token);
            if (buffer.Array == null) continue;
            jsonResult.Append(Encoding.UTF8.GetString(buffer.Array, 0, taskResult.Count));

        } while (!taskResult.EndOfMessage);

        return jsonResult.ToString();
    }
    static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
