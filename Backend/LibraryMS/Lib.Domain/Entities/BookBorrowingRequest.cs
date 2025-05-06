using Lib.Domain.Enum;

namespace Lib.Domain.Entities;

public class BookBorrowingRequest
{
    public Guid Id { get; set; }
    public Guid RequestorId { get; set; }
    public User Requestor { get; set; } = null!;
    public DateTime RequestedDate { get; set; }
    public RequestStatus Status { get; set; }
    public Guid ?ApprovedId { get; set; }
    public User ?Approver { get; set; }
    public ICollection<BookBorrowingRequestDetails> Details { get; set; } = [];
}
