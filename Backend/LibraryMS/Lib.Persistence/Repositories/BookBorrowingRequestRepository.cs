using Lib.Domain.Entities;
using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Persistence.Repositories;

public class BookBorrowingRequestRepository : Repository<BookBorrowingRequest>, IBookBorrowingRequestRepository
{
    public BookBorrowingRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> GetRequestInMonth(Guid userId, int month)
    {
        return await _context.BookBorrowingRequests
            .CountAsync(r => r.RequestorId == userId && r.RequestedDate.Month == month);
    }
}
