using System;

namespace ServiceClient.DTOs
{
    public class WebSocketOptions
    {
        public string Url { get; set; }
        public int ConnectionTimeoutInMilliseconds { get; set; }
        public int KeepAliveIntervalInSeconds { get; set; }
    }
}
