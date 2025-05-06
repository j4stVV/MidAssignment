namespace Lib.Application.Dtos.Borrowing;

public class CreateBorrowingRequestDto
{
    public List<Guid> BookIds { get; set; } = [];
}
