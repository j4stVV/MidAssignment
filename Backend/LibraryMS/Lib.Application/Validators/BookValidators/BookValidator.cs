using FluentValidation;
using Lib.Domain.Entities;

public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(b => b.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(50).WithMessage("Author must not exceed 50 characters.");

        RuleFor(b => b.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

        RuleFor(b => b.ISBN)
            .NotEmpty().WithMessage("ISBN is required.");

        RuleFor(b => b.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.");

        RuleFor(b => b.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

        RuleFor(b => b.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}