using Lib.Domain.Entities;
using Lib.Domain.Enum;

namespace Lib.Application.Dtos.Borrowing;

public class BorrowingRequestDto
{
    public Guid Id { get; set; }
    public Guid RequestorId { get; set; }
    public string RequestorName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public RequestStatus Status { get; set; }
    public Guid ApprovedId { get; set; }
    public string? ApproverName { get; set; }
    public ICollection<BookBorrowingRequestDetailsDto> Details { get; set; } = [];
}
