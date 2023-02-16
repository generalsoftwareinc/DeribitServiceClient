using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using ServiceClient.Implements;
using ServiceClient.Implements.SocketClient.DTOs;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceClient.Implements.SocketClient;

internal class DeribitSocketClient : IDeribitClient
{
    private int nextId = 0;
    private const int readBufferSize = 100;
    private readonly ClientWebSocket webSocket;
    private readonly DeribitOptions deribitOptions;
    public event EventHandler<BookReadedEventArgs>? OnBookReaded;
    public event EventHandler<TickerReadedEventArgs>? OnTickerReaded;

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

    private StringBuilder stringBuilder = new();
    private byte[] readBuffer = new byte[readBufferSize];
    private WebSocketState[] receiveValidStates = new[] { WebSocketState.Open, WebSocketState.CloseSent };

    private async Task<string> ReadStringAsync(CancellationToken cancellationToken)
    {
        WebSocketReceiveResult response;        
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
        await ReadAsync<object>(token);
    }

    public async Task AuthenticateAsync(CancellationToken token)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            await ConnectAsync(token);
        }
        var data = new
        {
            grant_type = "client_credentials",
            client_id = deribitOptions.ClientId,
            client_secret = deribitOptions.ClientSecret,
        };
        await SendAsync("public/auth", data, token);
        var credentials = await ReadAsync<AuthResult>(token);
        Credentials = credentials?.Result;
        await SetHeartbeatAsync(token);
    }

    public async Task SetHeartbeatAsync(CancellationToken token)
    {
        var data = new
        {
            interval = deribitOptions.HeartBeatInterval
        };
        await SendAsync("public/set_heartbeat", data, token);
        await ReadAsync<object>(token);
    }

    public async Task DisableHeartbeatAsync(CancellationToken token)
    {
        await SendAsync("public/disable_heartbeat", new object(), token);
        await ReadAsync<object>(token);
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
        while (!token.IsCancellationRequested)
        {
            var jsonResult = await ReadStringAsync(token);

            var isBookMessage = jsonResult.IndexOf(BookChannel) >= 0;
            var isTikerMessage = jsonResult.IndexOf(TickerChannel) >= 0;

            if (isBookMessage)
            {
                var book = JsonSerializer.Deserialize<BookResponse>(jsonResult);
                if (book is not null)
                {
                    OnBookReaded?.Invoke(this, new BookReadedEventArgs(book));
                }
            }
            else if (isTikerMessage)
            {
                var ticker = JsonSerializer.Deserialize<TickerResponse>(jsonResult);
                if (ticker is not null)
                {
                    OnTickerReaded?.Invoke(this, new TickerReadedEventArgs(ticker));
                }
            }
            else if (jsonResult.Contains("test_request"))
            {
                await SendTestAsync(token);
            }
        }
    }

    static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
