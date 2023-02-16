namespace ServiceClient.Implements.SocketClient.DTOs;

public class DeribitSocketOptions
{
    public string Url { get; set; } = string.Empty;
    public int ConnectionTimeoutInMilliseconds { get; set; }
    public int KeepAliveIntervalInSeconds { get; set; }
}
