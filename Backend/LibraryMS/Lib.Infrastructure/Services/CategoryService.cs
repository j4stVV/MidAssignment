using AutoMapper;
using FluentValidation;
using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos;
using Lib.Application.Dtos.CategoryDto;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<Category> _validator;
    private readonly IValidator<CreateCategoryDto> _createCategoryValidator;
    private readonly IValidator<UpdateCategoryDto> _updateCategoryValidator;
    public CategoryService(ICategoryRepository categoryRepository,
        IMapper mapper,
        IValidator<Category> validator,
        IValidator<CreateCategoryDto> createCategoryValidator,
        IValidator<UpdateCategoryDto> updateCategoryValidator)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _validator = validator;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found");
        return _mapper.Map<CategoryDto>(category);
    }
    public async Task CreateCategoryAsync(CreateCategoryDto dto)
    {
        await _createCategoryValidator.ValidateAndThrowAsync(dto);

        var category = _mapper.Map<Category>(dto);

        var existingCategory = await _categoryRepository.Query()
                .FirstOrDefaultAsync(c => c.Name == category.Name);
        if (existingCategory != null)
            throw new BusinessRuleException($"Category with name {category.Name} already exists.");

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.Id);
        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        await _updateCategoryValidator.ValidateAndThrowAsync(dto);
        _mapper.Map(dto, category);

        var existingCategory = await _categoryRepository.Query()
               .FirstOrDefaultAsync(c => c.Name == category.Name && c.Id != category.Id);
        if (existingCategory != null)
            throw new BusinessRuleException($"Category with name {category.Name} already exists.");

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            throw new NotFoundException($"Category with ID {id} not found.");
        if (category.Books.Any())
            throw new BusinessRuleException("Cannot delete category with associated books.");
        await _categoryRepository.DeleteAsync(category);
    }

    public async Task<PagedResultDto<CategoryDto>> GetCategoriesAsync(int pageNumber = 1, int pageSize = 5)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.");
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0.");

        var query = await _categoryRepository.Query()
            .AsNoTracking()
            .ToListAsync();

        var totalItems = query.Count();
        var items = query
            .OrderBy(c => c.Name) // Sort by name
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<CategoryDto>
        {
            Items = _mapper.Map<List<CategoryDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
