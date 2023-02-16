namespace ServiceClient.Implements.SocketClient.DTOs;

public sealed class DeribitOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
    public int ConnectionTimeoutInMilliseconds { get; set; }
    public int KeepAliveIntervalInSeconds { get; set; }
    public string InstrumentName { get; set; } = string.Empty;
    public string TickerInterval { get; set; } = string.Empty;
    public string BookInterval { get; set; } = string.Empty;
    public int HeartBeatInterval { get; set; }
}
