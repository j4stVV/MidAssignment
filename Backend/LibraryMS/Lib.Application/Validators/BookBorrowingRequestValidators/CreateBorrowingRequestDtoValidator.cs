using FluentValidation;
using Lib.Application.Dtos.Borrowing;
namespace Lib.Application.Validators.BookBorrowingRequestValidators;

public class CreateBorrowingRequestDtoValidator : AbstractValidator<CreateBorrowingRequestDto>
{
    public CreateBorrowingRequestDtoValidator()
    {
        RuleFor(dto => dto.BookIds)
            .NotEmpty().WithMessage("At least one book ID is required.")
            .Must(ids => ids.Count <= 5).WithMessage("Cannot borrow more than 5 books in one request.");
    }
}
