using Deribit.ServiceClient.DTOs.Ticker;

namespace Deribit.ServiceClient.DTOs
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
