namespace Lib.Application.Dtos.CategoryDto;

public class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}