using Deribit.ServiceClient.DTOs.Book;

namespace Deribit.ServiceClient.DTOs;

internal sealed class BookReadedEventArgs
{
    public BookReadedEventArgs(BookResponse read)
    {
        Read = read;
    }

    public BookResponse Read { get; }
}
