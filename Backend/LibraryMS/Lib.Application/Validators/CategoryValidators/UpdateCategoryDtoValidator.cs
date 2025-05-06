using FluentValidation;
using Lib.Application.Dtos.CategoryDto;

namespace Lib.Application.Validators.CategoryValidators;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(dto => dto.Id)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters.");
    }
}
