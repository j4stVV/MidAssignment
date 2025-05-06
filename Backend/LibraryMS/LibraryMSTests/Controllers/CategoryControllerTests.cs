using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.CategoryDto;
using Lib.Application.Dtos;
using LibraryManament.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMSTests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _controller = new CategoryController(_mockCategoryService.Object);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOkWithData()
    {
        // Arrange
        var categories = new List<CategoryDto> { new() { Id = Guid.NewGuid(), Name = "Test" } };
        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(categories, okResult.Value);
    }

    [Fact]
    public async Task GetCategories_ReturnsPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResultDto<CategoryDto>
        {
            Items = new List<CategoryDto> { new() { Id = Guid.NewGuid(), Name = "Paged" } },
            PageNumber = 1,
            PageSize = 5,
            TotalItems = 1
        };
        _mockCategoryService.Setup(x => x.GetCategoriesAsync(1, 5)).ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(pagedResult, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsCategory()
    {
        var id = Guid.NewGuid();
        var category = new CategoryDto { Id = id, Name = "Single" };
        _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(id)).ReturnsAsync(category);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(category, okResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsOk_WhenSuccess()
    {
        var dto = new CreateCategoryDto { Name = "New Category" };
        _mockCategoryService.Setup(x => x.CreateCategoryAsync(dto)).Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = GetMessageFromResult(okResult);
        Assert.Equal("Category created successfully.", message);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        var dto = new UpdateCategoryDto { Id = Guid.NewGuid(), Name = "Updated" };
        _mockCategoryService.Setup(x => x.UpdateCategoryAsync(dto)).Returns(Task.CompletedTask);

        var result = await _controller.Update(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = GetMessageFromResult(okResult);
        Assert.Equal("Category updated successfully.", message);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenSuccess()
    {
        var id = Guid.NewGuid();
        _mockCategoryService.Setup(x => x.DeleteCategoryAsync(id)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = GetMessageFromResult(okResult);
        Assert.Equal("Category deleted successfully.", message);
    }

    private static string GetMessageFromResult(OkObjectResult result)
    {
        // Uses reflection because anonymous types don't play well with dynamic casting
        return result.Value?.GetType().GetProperty("Message")?.GetValue(result.Value)?.ToString()!;
    }
}