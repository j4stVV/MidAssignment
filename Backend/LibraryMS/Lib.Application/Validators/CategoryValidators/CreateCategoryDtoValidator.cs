using FluentValidation;
using Lib.Application.Dtos.CategoryDto;

namespace Lib.Application.Validators.CategoryValidators;

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters.");
    }
}
