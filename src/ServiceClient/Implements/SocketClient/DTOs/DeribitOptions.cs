﻿using System.ComponentModel.DataAnnotations;

namespace ServiceClient.Implements.SocketClient.DTOs;

public sealed class DeribitOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    [Required]
    public string WebSocketUrl { get; set; } = string.Empty;
    [Required]
    [Range(1, 5000)]
    public int ConnectionTimeoutInMilliseconds { get; set; }
    [Required]
    [Range(1, 100)]
    public int KeepAliveIntervalInSeconds { get; set; }
    [Required]
    public string InstrumentName { get; set; } = string.Empty;
    [Required]
    public string TickerInterval { get; set; } = string.Empty;
    [Required]
    public string BookInterval { get; set; } = string.Empty;
    [Required]
    [Range(1, 3000)]
    public int HeartBeatInterval { get; set; }
}
