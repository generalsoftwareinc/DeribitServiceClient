namespace ServiceClient.Implements.DTOs
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
