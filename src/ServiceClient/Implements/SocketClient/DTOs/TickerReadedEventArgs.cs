namespace ServiceClient.Implements.SocketClient.DTOs
{
    internal sealed class TickerReadedEventArgs
    {
        public TickerReadedEventArgs(TickerResponse readed)
        {
            Readed = readed;
        }

        public TickerResponse Readed { get; }
    }
}
