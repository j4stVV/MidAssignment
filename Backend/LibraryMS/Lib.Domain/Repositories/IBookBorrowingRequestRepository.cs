using Lib.Domain.Entities;

namespace Lib.Domain.Repositories;

public interface IBookBorrowingRequestRepository : IRepository<BookBorrowingRequest>
{
    Task<int> GetRequestInMonth(Guid userId, int month);
}
