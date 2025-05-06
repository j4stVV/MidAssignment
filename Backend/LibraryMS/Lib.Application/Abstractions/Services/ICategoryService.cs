using Lib.Application.Dtos;
using Lib.Application.Dtos.CategoryDto;

namespace Lib.Application.Abstractions.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task CreateCategoryAsync(CreateCategoryDto dto);
    Task UpdateCategoryAsync(UpdateCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);
    Task<PagedResultDto<CategoryDto>> GetCategoriesAsync(int pageNumber = 1, int pageSize = 10);
}
