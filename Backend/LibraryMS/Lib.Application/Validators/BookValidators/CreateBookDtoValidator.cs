using FluentValidation;
using Lib.Application.Dtos.BookDTOs;
using Lib.Domain.Repositories;

namespace Lib.Application.Validators.BookValidators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateBookDtoValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Length(2, 100).WithMessage("Title must be between 2 and 100 characters.");

        RuleFor(dto => dto.Author)
            .NotEmpty().WithMessage("Author is required.")
            .Length(2, 50).WithMessage("Author must be between 2 and 50 characters.");

        RuleFor(dto => dto.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Length(10, 20).WithMessage("ISBN must be between 10 and 20 characters.");

        RuleFor(dto => dto.PublishedDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.");

        RuleFor(dto => dto.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");

        RuleFor(dto => dto.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");
    }
}
