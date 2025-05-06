

using Lib.Application.Dtos.Borrowing;

namespace Lib.Application.Abstractions.Services;

public interface IBookBorrowingRequestService
{
    Task<BorrowingRequestDto> CreateBorrowingRequestAsync(CreateBorrowingRequestDto requestDto, Guid userId);
    Task<List<BorrowingRequestDto>> GetUserBorrowingRequestsAsync(Guid userId);
    Task<List<BorrowingRequestDto>> GetAllBorrowingRequestsAsync();
    Task ApproveBorrowingRequestAsync(Guid requestId, Guid approverId);
    Task RejectBorrowingRequestAsync(Guid requestId, Guid approverId);

}
