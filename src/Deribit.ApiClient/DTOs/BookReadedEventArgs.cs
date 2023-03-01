using Deribit.ApiClient.DTOs.Book;

namespace Deribit.ApiClient.DTOs;

internal sealed class BookReadedEventArgs
{
    public BookReadedEventArgs(BookResponse read)
    {
        Read = read;
    }

    public BookResponse Read { get; }
}
