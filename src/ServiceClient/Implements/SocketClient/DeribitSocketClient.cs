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

    private void Timer_Elapsed(object? state)
    {
        refreshTokenTimer?.Dispose();
        AuthenticateWithRefreshTokenAsync(CancellationToken.None).Wait();
    }

    private Task SendTestAsync(CancellationToken token)
    {
        return SendAsync("public/test", new object(), token);
    }

    private Task SendAsync<T>(string method, T data, CancellationToken cancellationToken)
    {
        var request = new Request<T>
        {
            JsonRpcVersion = "2.0",
            Id = ++nextId,
            Method = method,
            Parameters = data,
        };
        var jsonMessage = JsonSerializer.Serialize(request, jsonOptions);
        var buffer = new byte[jsonMessage.Length];

        Encoding.Default.GetBytes(jsonMessage, 0, jsonMessage.Length, buffer, 0);

        return webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    static private readonly byte[] readBuffer = new byte[readBufferSize];
    static private readonly IEnumerable<WebSocketState> receiveValidStates = new[] { WebSocketState.Open, WebSocketState.CloseSent };
    private readonly StringBuilder stringBuilder = new();
    
    private async Task<string> ReadStringAsync(CancellationToken cancellationToken)
    {
        WebSocketReceiveResult response;
        stringBuilder.Clear();
        do
        {
            if (!receiveValidStates.Contains(webSocket.State)) return string.Empty;
            response = await webSocket.ReceiveAsync(readBuffer, cancellationToken);
            var result = Encoding.Default.GetString(readBuffer, 0, response.Count);
            stringBuilder.Append(result);
        }
        while (!response.EndOfMessage);

        return stringBuilder.ToString();
    }

    private async Task ConnectAsync(CancellationToken token)
    {
        if (webSocket.State == WebSocketState.Open) return;
        await webSocket.ConnectAsync(new Uri(deribitOptions.WebSocketUrl), token);
    }

    private async Task<ActionResponse<T>?> ReadAsync<T>(CancellationToken token)
        where T : class
    {
        var result = await ReadStringAsync(token);
        return JsonSerializer.Deserialize<ActionResponse<T>>(result, jsonOptions);
    }

    public async Task DisconnectAsync(CancellationToken token)
    {
        await DisableHeartbeatAsync(token);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
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
        var credentials = await ReadStringAsync(token);
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
        return SendAsync("public/set_heartbeat", data, token);
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
        await ReadAsync<object>(token);
    }

    public async Task ContinueReadAsync(CancellationToken token)
    {
        _ = Task.Run(() => ReceiveMessageAsync(webSocket, token), token);
        while (!token.IsCancellationRequested)
        {
            var message = readMessageQueue.Take(token);
            var isBookMessage = message.Contains(BookChannel, StringComparison.CurrentCulture);
            var isTikerMessage = message.Contains(TickerChannel, StringComparison.CurrentCulture);

            if (isBookMessage)
            {
                var book = JsonSerializer.Deserialize<BookResponse>(message);
                if (book is not null)
                {
                    OnBookReaded?.Invoke(this, new BookReadedEventArgs(book));
                }
            }
            else if (isTikerMessage)
            {
                var ticker = JsonSerializer.Deserialize<TickerResponse>(message);
                if (ticker is not null)
                {
                    OnTickerReaded?.Invoke(this, new TickerReadedEventArgs(ticker));
                }
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

    private void ParseCredentials(string json)
    {
        refreshTokenTimer?.Dispose();
        var credentials = JsonSerializer.Deserialize<ActionResponse<AuthResult>>(json, jsonOptions);
        Credentials = credentials?.Result;
        if (Credentials?.ExpiresIn != null)
        {
            refreshTokenTimer = new Timer(Timer_Elapsed);
            var runAt = Credentials!.ExpiresIn! - 300;
            refreshTokenTimer.Change(runAt, runAt);
        }
    }

    static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    async static Task ReceiveMessageAsync(ClientWebSocket ws, CancellationToken token)
    {
        var buffer = WebSocket.CreateClientBuffer(1024, 1024);
        WebSocketReceiveResult taskResult;
        var jsonResult = new StringBuilder();

        while (ws.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            jsonResult.Clear();
            do
            {
                taskResult = await ws.ReceiveAsync(buffer, token);
                if (buffer.Array == null) continue;
                jsonResult.Append(Encoding.UTF8.GetString(buffer.Array, 0, taskResult.Count));

            } while (!taskResult.EndOfMessage);

            if (jsonResult.Length > 0)
            {
                readMessageQueue.Add(jsonResult.ToString(), token);
            }
        }
    }

    class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
