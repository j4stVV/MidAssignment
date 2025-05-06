using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Lib.Application.Dtos.CategoryDto;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Lib.Domain.Repositories;
using Lib.Infrastructure.Services;
using MockQueryable.Moq;
using Moq;

namespace LibraryMSTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<Category>> _validatorMock;
    private readonly Mock<IValidator<CreateCategoryDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateCategoryDto>> _updateValidatorMock;
    private readonly CategoryService _categoryService;
    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<Category>>();
        _createValidatorMock = new Mock<IValidator<CreateCategoryDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateCategoryDto>>();

        _categoryService = new CategoryService(
            _categoryRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Category1" },
            new Category { Id = Guid.NewGuid(), Name = "Category2" }
        };
        var categoryDtos = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
        _categoryRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);
        _mapperMock.Setup(x => x.Map<IEnumerable<CategoryDto>>(categories)).Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        Assert.Equal(categoryDtos, result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ValidId_ReturnsCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Category1" };
        var categoryDto = new CategoryDto { Id = categoryId, Name = "Category1" };
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _mapperMock.Setup(x => x.Map<CategoryDto>(category)).Returns(categoryDto);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(categoryId);

        // Assert
        Assert.Equal(categoryDto, result);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_InvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync((Category)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.GetCategoryByIdAsync(categoryId));
    }

    [Fact]
    public async Task CreateCategoryAsync_ValidDto_CreatesCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "New Category" };
        var category = new Category { Id = Guid.NewGuid(), Name = "New Category" };
        var categories = new List<Category>();
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<Category>(dto)).Returns(category);
        _categoryRepositoryMock.Setup(x => x.AddAsync(category)).Returns(Task.CompletedTask);
        _categoryRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _categoryService.CreateCategoryAsync(dto);

        // Assert
        _categoryRepositoryMock.Verify(x => x.AddAsync(category), Times.Once());
        _categoryRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task CreateCategoryAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "" };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") });
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.CreateCategoryAsync(dto));
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateName_ThrowsBusinessRuleException()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Existing Category" };
        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Existing Category" };
        var categories = new List<Category> { existingCategory };
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<Category>(dto)).Returns(new Category { Name = "Existing Category" });

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _categoryService.CreateCategoryAsync(dto));
    }

    [Fact]
    public async Task UpdateCategoryAsync_ValidDto_UpdatesCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new UpdateCategoryDto { Id = categoryId, Name = "Updated Category" };
        var category = new Category { Id = categoryId, Name = "Old Category" };
        var categories = new List<Category>();
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _updateValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map(dto, category));
        _categoryRepositoryMock.Setup(x => x.Update(category)).Verifiable();
        _categoryRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _categoryService.UpdateCategoryAsync(dto);

        // Assert
        _categoryRepositoryMock.Verify(x => x.Update(category), Times.Once());
        _categoryRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task UpdateCategoryAsync_InvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateCategoryDto { Id = Guid.NewGuid(), Name = "Updated Category" };
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync((Category)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.UpdateCategoryAsync(dto));
    }

    [Fact]
    public async Task UpdateCategoryAsync_DuplicateName_ThrowsBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new UpdateCategoryDto { Id = categoryId, Name = "Existing Category" };
        var category = new Category { Id = categoryId, Name = "Old Category" };
        var existingCategory = new Category { Id = Guid.NewGuid(), Name = "Existing Category" };
        var categories = new List<Category> { existingCategory };
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId)).ReturnsAsync(category);
        _updateValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map(dto, category)).Callback(() => category.Name = "Existing Category");

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _categoryService.UpdateCategoryAsync(dto));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ValidIdNoBooks_DeletesCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Category1", Books = new List<Book>() };
        var categories = new List<Category> { category };
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _categoryRepositoryMock.Setup(x => x.DeleteAsync(category)).Returns(Task.CompletedTask);

        // Act
        await _categoryService.DeleteCategoryAsync(categoryId);

        // Assert
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(category), Times.Once());
    }

    [Fact]
    public async Task DeleteCategoryAsync_InvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categories = new List<Category>();
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _categoryService.DeleteCategoryAsync(categoryId));
    }

    [Fact]
    public async Task DeleteCategoryAsync_HasBooks_ThrowsBusinessRuleException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Category1",
            Books = new List<Book> { new Book() }
        };
        var categories = new List<Category> { category };
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _categoryService.DeleteCategoryAsync(categoryId));
    }

    [Fact]
    public async Task GetCategoriesAsync_ValidPage_ReturnsPagedCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Category1" },
            new Category { Id = Guid.NewGuid(), Name = "Category2" }
        };
        var categoryDtos = categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
        var mockQueryable = categories.AsQueryable().BuildMockDbSet();
        _categoryRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<CategoryDto>>(It.IsAny<List<Category>>())).Returns(categoryDtos);

        // Act
        var result = await _categoryService.GetCategoriesAsync(1, 5);

        // Assert
        Assert.Equal(categoryDtos, result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetCategoriesAsync_InvalidPageNumber_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.GetCategoriesAsync(0, 5));
    }
}