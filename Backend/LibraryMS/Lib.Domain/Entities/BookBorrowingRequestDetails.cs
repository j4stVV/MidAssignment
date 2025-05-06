namespace Lib.Domain.Entities;

public class BookBorrowingRequestDetails
{
    public Guid Id { get; set; }
    public Guid BookBorrowingRequestId { get; set; }
    public BookBorrowingRequest BookBorrowingRequest { get; set; } = null!;
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
}
