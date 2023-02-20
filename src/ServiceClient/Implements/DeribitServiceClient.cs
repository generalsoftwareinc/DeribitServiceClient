﻿using Microsoft.Extensions.Logging;
using ServiceClient.Abstractions;
using ServiceClient.Implements.SocketClient.DTOs;

namespace ServiceClient.Implements;

internal class DeribitServiceClient : IServiceClient
{
    private readonly IDeribitClient deribitSocket;
    private readonly ILogger<DeribitServiceClient> logger;
    private BookData? lastBook;

    public DeribitServiceClient(IDeribitClient deribitSocket, ILogger<DeribitServiceClient> logger)
    {
        this.deribitSocket = deribitSocket;
        this.logger = logger;
    }

    public event TickerReceivedEventHandler? OnTickerReceived;
    public async Task<bool> DisconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await deribitSocket.DisconnectAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurs {error}", ex);
            return false;
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await deribitSocket.CheckAvailabilityAsync(cancellationToken);
            await deribitSocket.InitializeAsync(cancellationToken);
            await deribitSocket.SubscribeAsync(cancellationToken);
            deribitSocket.OnBookReaded += DeribitSocket_OnBookReaded;
            deribitSocket.OnTickerReaded += DeribitSocket_OnTickerReaded;

            await deribitSocket.ContinueReadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurs {error}", ex);
        }
    }

    private void DeribitSocket_OnTickerReaded(object? sender, TickerReadedEventArgs e)
    {
        OnTickerReceived?.Invoke(this, new TickerReceivedEventArgs(e.Read.Parameters?.Data, lastBook));
    }

    private void DeribitSocket_OnBookReaded(object? sender, BookReadedEventArgs e)
    {
        lastBook = e.Read.Parameters?.Data;
    }

    public async Task IsDeribitAvailableAsync(CancellationToken cancellationToken)
    {
        await deribitSocket.CheckAvailabilityAsync(cancellationToken);
    }
}
