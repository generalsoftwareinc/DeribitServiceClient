namespace ServiceClient.Implements.DTOs;

internal sealed class BookReadedEventArgs
{
    public BookReadedEventArgs(BookResponse read)
    {
        Read = read;
    }

    public BookResponse Read { get; }
}
