using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Lib.Application.Dtos.BookDTOs;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Lib.Domain.Repositories;
using Lib.Infrastructure.Services;
using Moq;
using MockQueryable.Moq;

namespace LibraryMSTests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<Book>> _validatorMock;
    private readonly Mock<IValidator<CreateBookDto>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateBookDto>> _updateValidatorMock;
    private readonly BookService _bookService;
    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<Book>>();
        _createValidatorMock = new Mock<IValidator<CreateBookDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateBookDto>>();

        _bookService = new BookService(
            _bookRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = Guid.NewGuid(), Title = "Book1", Category = new Category() },
            new Book { Id = Guid.NewGuid(), Title = "Book2", Category = new Category() }
        };
        var bookDtos = books.Select(b => new BookDto { Id = b.Id, Title = b.Title }).ToList();
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<BookDto>>(books)).Returns(bookDtos);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        Assert.Equal(bookDtos, result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBooksAsync_ValidPage_ReturnsPagedBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = Guid.NewGuid(), Title = "Book1", Category = new Category() },
            new Book { Id = Guid.NewGuid(), Title = "Book2", Category = new Category() }
        };
        var bookDtos = books.Select(b => new BookDto { Id = b.Id, Title = b.Title }).ToList();
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<BookDto>>(It.IsAny<List<Book>>())).Returns(bookDtos);

        // Act
        var result = await _bookService.GetBooksAsync(1, 5);

        // Assert
        Assert.Equal(bookDtos, result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetBooksAsync_InvalidPageNumber_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _bookService.GetBooksAsync(0, 5));
    }

    [Fact]
    public async Task GetBookByIdAsync_ValidId_ReturnsBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Book1", Category = new Category() };
        var bookDto = new BookDto { Id = bookId, Title = "Book1" };
        var books = new List<Book> { book };
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<BookDto>(book)).Returns(bookDto);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        Assert.Equal(bookDto, result);
    }

    [Fact]
    public async Task GetBookByIdAsync_InvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var books = new List<Book>();
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.GetBookByIdAsync(bookId));
    }

    [Fact]
    public async Task CreateBookAsync_ValidDto_CreatesBook()
    {
        // Arrange
        var dto = new CreateBookDto { Title = "New Book", CategoryId = Guid.NewGuid() };
        var book = new Book { Id = Guid.NewGuid(), Title = "New Book", CategoryId = dto.CategoryId };
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category());
        _mapperMock.Setup(x => x.Map<Book>(dto)).Returns(book);
        _bookRepositoryMock.Setup(x => x.AddAsync(book)).Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _bookService.CreateBookAsync(dto);

        // Assert
        _bookRepositoryMock.Verify(x => x.AddAsync(book), Times.Once());
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task CreateBookAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateBookDto { Title = "", CategoryId = Guid.NewGuid() };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Title is required") });
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _bookService.CreateBookAsync(dto));
    }

    [Fact]
    public async Task CreateBookAsync_InvalidCategory_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateBookDto { Title = "New Book", CategoryId = Guid.NewGuid() };
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync((Category)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.CreateBookAsync(dto));
    }

    [Fact]
    public async Task UpdateBookAsync_ValidDto_UpdatesBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var dto = new UpdateBookDto { Id = bookId, Title = "Updated Book", CategoryId = Guid.NewGuid(), Quantity = 10 };
        var book = new Book { Id = bookId, Title = "Old Book", CategoryId = Guid.NewGuid(), Quantity = 5, Available = 5 };
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);
        _updateValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category());
        _mapperMock.Setup(x => x.Map(dto, book));
        _bookRepositoryMock.Setup(x => x.Update(book)).Verifiable();
        _bookRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _bookService.UpdateBookAsync(dto);

        // Assert
        _bookRepositoryMock.Verify(x => x.Update(book), Times.Once());
        _bookRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        Assert.Equal(5, book.Available); // Available = (dto.Quantity - book.Quantity) + book.Available
    }

    [Fact]
    public async Task UpdateBookAsync_InvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateBookDto { Id = Guid.NewGuid(), Title = "Updated Book", CategoryId = Guid.NewGuid() };
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync((Book)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.UpdateBookAsync(dto));
    }

    [Fact]
    public async Task UpdateBookAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var dto = new UpdateBookDto { Id = bookId, Title = "", CategoryId = Guid.NewGuid() };
        var book = new Book { Id = bookId, Title = "Old Book" };
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Title is required") });
        _updateValidatorMock.Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _bookService.UpdateBookAsync(dto));
    }

    [Fact]
    public async Task DeleteBookAsync_ValidIdNoBorrowing_DeletesBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Book1", BorrowingDetails = new List<BookBorrowingRequestDetails>() };
        var books = new List<Book> { book };
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _bookRepositoryMock.Setup(x => x.DeleteAsync(book)).Returns(Task.CompletedTask);

        // Act
        await _bookService.DeleteBookAsync(bookId);

        // Assert
        _bookRepositoryMock.Verify(x => x.DeleteAsync(book), Times.Once());
    }

    [Fact]
    public async Task DeleteBookAsync_HasBorrowing_ThrowsBusinessRuleException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Book1",
            BorrowingDetails = new List<BookBorrowingRequestDetails> { new BookBorrowingRequestDetails() }
        };
        var books = new List<Book> { book };
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _bookService.DeleteBookAsync(bookId));
    }

    [Fact]
    public async Task GetFilteredBooksAsync_ValidFilters_ReturnsFilteredBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = Guid.NewGuid(), Title = "Test Book", Author = "Author1", CategoryId = Guid.NewGuid(), Available = 1, Category = new Category() }
        };
        var bookDtos = books.Select(b => new BookDto { Id = b.Id, Title = b.Title }).ToList();
        var mockQueryable = books.AsQueryable().BuildMockDbSet();
        _bookRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<BookDto>>(It.IsAny<List<Book>>())).Returns(bookDtos);

        // Act
        var result = await _bookService.GetFilteredBooksAsync("Test", "Author1", books[0].CategoryId, true, 1, 5);

        // Assert
        Assert.Equal(bookDtos, result.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task GetFilteredBooksAsync_InvalidPageNumber_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _bookService.GetFilteredBooksAsync(null, null, null, null, 0, 5));
    }
}