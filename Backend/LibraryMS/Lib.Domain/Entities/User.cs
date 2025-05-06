using Microsoft.AspNetCore.Identity;

namespace Lib.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public ICollection<BookBorrowingRequest> BookBorrowingRequests { get; set; } = [];
    public ICollection<BookBorrowingRequest> ApprovedRequests { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
