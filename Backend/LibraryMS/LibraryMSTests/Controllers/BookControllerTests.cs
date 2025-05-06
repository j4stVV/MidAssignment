using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos;
using Lib.Application.Dtos.BookDTOs;
using LibraryManament.Controller;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.Dynamic;

namespace LibraryMSTests.Controllers;

public class BookControllerTests
{
    private readonly Mock<IBookService> _mockBookService;
    private readonly BookController _controller;

    public BookControllerTests()
    {
        _mockBookService = new Mock<IBookService>();
        _controller = new BookController(_mockBookService.Object);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WithBooks()
    {
        // Arrange
        var mockBooks = new List<BookDto>
        {
            new BookDto { Id = Guid.NewGuid(), Title = "Book 1" },
            new BookDto { Id = Guid.NewGuid(), Title = "Book 2" }
        };
        _mockBookService.Setup(service => service.GetAllBooksAsync())
            .ReturnsAsync(mockBooks);

        // Act
        var result = await _controller.GetAllBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var books = Assert.IsType<List<BookDto>>(okResult.Value);
        Assert.Equal(mockBooks.Count, books.Count);
    }

    [Fact]
    public async Task GetBooks_ReturnsOk_WithPagedBooks()
    {
        // Arrange
        var mockBooks = new List<BookDto>
    {
        new BookDto { Id = Guid.NewGuid(), Title = "Book 1" },
        new BookDto { Id = Guid.NewGuid(), Title = "Book 2" }
    };
        var pagedResult = new PagedResultDto<BookDto>
        {
            Items = mockBooks,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = mockBooks.Count
        };
        _mockBookService.Setup(service => service.GetBooksAsync(1, 5))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetBooks(1, 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PagedResultDto<BookDto>>(okResult.Value);
        Assert.Equal(mockBooks.Count, response.Items.Count);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WithBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var mockBook = new BookDto { Id = bookId, Title = "Test Book" };
        _mockBookService.Setup(service => service.GetBookByIdAsync(bookId))
            .ReturnsAsync(mockBook);

        // Act
        var result = await _controller.GetById(bookId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var book = Assert.IsType<BookDto>(okResult.Value);
        Assert.Equal(bookId, book.Id);
    }

    [Fact]
    public async Task Create_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var bookDto = new CreateBookDto
        {
            Title = "Test Book",
            Author = "Test Author",
            Quantity = 10,
            CategoryId = Guid.NewGuid()
        };

        _mockBookService.Setup(s => s.CreateBookAsync(bookDto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(bookDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Deserialize anonymous object to dictionary
        var json = JsonConvert.SerializeObject(okResult.Value);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("Book created successfully.", dict["Message"]);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var bookDto = new UpdateBookDto
        {
            Id = Guid.NewGuid(),
            Title = "Updated Book",
            Author = "Updated Author",
            CategoryId = Guid.NewGuid(),
            Quantity = 15,
            Available = 10
        };
        _mockBookService.Setup(service => service.UpdateBookAsync(bookDto))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(bookDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonConvert.SerializeObject(okResult.Value);
        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("Book updated successfully.", response["Message"]);
    }


    [Fact]
    public async Task Delete_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _mockBookService.Setup(service => service.DeleteBookAsync(bookId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(bookId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonConvert.SerializeObject(okResult.Value);
        var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("Book deleted successfully.", response["Message"]);
    }


    [Fact]
    public async Task GetFilteredBooks_ReturnsOk_WithFilteredBooks()
    {
        // Arrange
        var mockBooks = new List<BookDto>
    {
        new BookDto { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1" },
        new BookDto { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2" }
    };
        var pagedResult = new PagedResultDto<BookDto>
        {
            Items = mockBooks,
            PageNumber = 1,
            PageSize = 5,
            TotalItems = mockBooks.Count
        };
        _mockBookService.Setup(service => service.GetFilteredBooksAsync("Book", "Author", null, null, 1, 5))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetFilteredBooks("Book", "Author", null, null, 1, 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PagedResultDto<BookDto>>(okResult.Value);
        Assert.Equal(mockBooks.Count, response.Items.Count);
    }

}
