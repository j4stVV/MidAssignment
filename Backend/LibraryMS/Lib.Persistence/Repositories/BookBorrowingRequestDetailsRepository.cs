using Lib.Domain.Entities;
using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Persistence.Repositories;

public class BookBorrowingRequestDetailsRepository : Repository<BookBorrowingRequestDetails>, IBookBorrowingRequestDetailsRepository
{
    public BookBorrowingRequestDetailsRepository(AppDbContext context) : base(context)
    {
        
    }
}
