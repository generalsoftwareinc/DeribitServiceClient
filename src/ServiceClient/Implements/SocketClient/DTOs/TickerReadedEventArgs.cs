namespace ServiceClient.Implements.SocketClient.DTOs
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
