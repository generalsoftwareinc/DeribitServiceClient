namespace ServiceClient.Implements.SocketClient.DTOs
{
    internal sealed class BookReadedEventArgs
    {
        public BookReadedEventArgs(BookResponse readed)
        {
            Readed = readed;
        }

        public BookResponse Readed { get; }
    }
}
