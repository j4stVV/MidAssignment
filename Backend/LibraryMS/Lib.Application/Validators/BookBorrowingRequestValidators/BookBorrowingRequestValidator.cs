using FluentValidation;
using Lib.Domain.Entities;

namespace Lib.Application.Validators.BookBorrowingRequestValidators;

public class BookBorrowingRequestValidator : AbstractValidator<BookBorrowingRequest>
{
    public BookBorrowingRequestValidator()
    {
        RuleFor(r => r.RequestorId)
                .NotEmpty().WithMessage("Requestor ID is required.");

        RuleFor(r => r.Details)
            .NotEmpty().WithMessage("At least one book must be included in the request.")
            .Must(d => d.Count <= 5).WithMessage("Cannot borrow more than 5 books in one request.")
            .Must(d => d.All(detail => detail.BookId != Guid.Empty)).WithMessage("All book selections must be valid.");
    }
}
