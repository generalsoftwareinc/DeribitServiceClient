using Deribit.ApiClient.DTOs.Ticker;

namespace Deribit.ApiClient.DTOs
{
    internal sealed class TickerReadedEventArgs
    {
        public TickerReadedEventArgs(TickerResponse read)
        {
            Read = read;
        }

        public TickerResponse Read { get; }
    }
}
