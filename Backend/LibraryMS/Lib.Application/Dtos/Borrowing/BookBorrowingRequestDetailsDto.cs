using Lib.Application.Dtos.BookDTOs;

namespace Lib.Application.Dtos.Borrowing;

public class BookBorrowingRequestDetailsDto
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
}
