using Deribit.ApiClient.DTOs;

namespace Deribit.ApiClient.Abstractions
{
    internal interface IDeribitClient
    {
        event EventHandler<BookReadedEventArgs>? OnBookReaded;
        event EventHandler<TickerReadedEventArgs>? OnTickerReaded;
        Task CheckAvailabilityAsync(CancellationToken token);
        Task InitializeAsync(CancellationToken token);
        Task SubscribeAsync(CancellationToken token);
        Task ContinueReadAsync(CancellationToken token);
        Task DisconnectAsync(CancellationToken token); 
    }
}
