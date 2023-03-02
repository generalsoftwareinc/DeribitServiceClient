using Deribit.ApiClient.Configuration;
using Deribit.ApiClient.DTOs;
using Deribit.ApiClient.DTOs.Auth;
using Deribit.ApiClient.DTOs.Book;
using Deribit.ApiClient.DTOs.Ticker;
using Deribit.ApiClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Deribit.ApiClient;

partial class DeribitApiClient
{
    #region RefreshToken timer
    private void StartNewRefreshTokenTimer(long runAtSec)
    {
        DisposeRefreshTokenTimer();
        if (isDisposingOrDisposed) return;

        refreshTokenTimer = new Timer(RefreshTokenTimerTicked);
        refreshTokenTimer.Change(runAtSec, runAtSec);
    }

    private void RefreshTokenTimerTicked(object? state)
    {
        DisposeRefreshTokenTimer();
        if (isDisposingOrDisposed) return;

        this.logger?.LogDebug("RefreshToken timer ticked");

        // TODO handle the case when Credentials is null!

        var message = GetAuthenticateByRefreshTokenMessage(Credentials!.RefreshToken);
        EnqueueOutgoingMessage(message, CancellationToken.None);
    }

    private void DisposeRefreshTokenTimer()
    {
        var timer = this.refreshTokenTimer;
        this.refreshTokenTimer = null;
        timer?.Dispose();
    }
    #endregion
}
