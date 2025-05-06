using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Lib.Application.Dtos.Borrowing;
using Lib.Domain.Entities;
using Lib.Domain.Enum;
using Lib.Domain.Repositories;
using Lib.Infrastructure.Services;
using MockQueryable.Moq;
using Moq;

namespace LibraryMSTests.Services;

public class BookBorrowingRequestServiceTests
{
    private readonly BookBorrowingRequestService _service;
    private readonly Mock<IBookBorrowingRequestRepository> _requestRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateBorrowingRequestDto>> _createValidatorMock;
    public BookBorrowingRequestServiceTests()
    {
        _requestRepositoryMock = new Mock<IBookBorrowingRequestRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<CreateBorrowingRequestDto>>();

        _service = new BookBorrowingRequestService(
            _requestRepositoryMock.Object,
            _bookRepositoryMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object);
    }

    [Fact]
    public async Task CreateBorrowingRequestAsync_ValidRequest_CreatesRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var requestDto = new CreateBorrowingRequestDto { BookIds = new List<Guid> { bookId } };
        var book = new Book { Id = bookId, Title = "Book1", Quantity = 1, Available = 1 };
        var request = new BookBorrowingRequest
        {
            Id = Guid.NewGuid(),
            RequestorId = userId,
            Status = RequestStatus.Waiting,
            Details = new List<BookBorrowingRequestDetails>
        {
            new BookBorrowingRequestDetails { Id = Guid.NewGuid(), BookId = bookId, Book = book }
        }
        };
        var requestDtoResult = new BorrowingRequestDto { Id = request.Id };

        _createValidatorMock.Setup(x => x.ValidateAsync(requestDto, default))
            .ReturnsAsync(new ValidationResult());
        _requestRepositoryMock.Setup(x => x.GetRequestInMonth(userId, It.IsAny<int>()))
            .ReturnsAsync(0);
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);
        _mapperMock.Setup(x => x.Map<BookBorrowingRequest>(It.IsAny<CreateBorrowingRequestDto>()))
            .Returns(request);
        _mapperMock.Setup(x => x.Map<BorrowingRequestDto>(It.IsAny<BookBorrowingRequest>()))
    .Returns(requestDtoResult);
        _requestRepositoryMock.Setup(x => x.AddAsync(It.IsAny<BookBorrowingRequest>()))
            .Returns(Task.CompletedTask);
        _bookRepositoryMock.Setup(x => x.Update(It.IsAny<Book>())).Verifiable();
        _requestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateBorrowingRequestAsync(requestDto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(requestDtoResult, result);
        Assert.Equal(0, book.Available); // Available should decrement
        _bookRepositoryMock.Verify(x => x.Update(book), Times.Once());
        _requestRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BookBorrowingRequest>()), Times.Once());
        _requestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task CreateBorrowingRequestAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestDto = new CreateBorrowingRequestDto { BookIds = new List<Guid>() };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("BookIds", "At least one book is required") });
        _createValidatorMock.Setup(x => x.ValidateAsync(requestDto, default))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateBorrowingRequestAsync(requestDto, userId));
    }

    [Fact]
    public async Task CreateBorrowingRequestAsync_ExceededMonthlyLimit_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestDto = new CreateBorrowingRequestDto { BookIds = new List<Guid> { Guid.NewGuid() } };
        _createValidatorMock.Setup(x => x.ValidateAsync(requestDto, default))
            .ReturnsAsync(new ValidationResult());
        _requestRepositoryMock.Setup(x => x.GetRequestInMonth(userId, It.IsAny<int>()))
            .ReturnsAsync(3);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateBorrowingRequestAsync(requestDto, userId));
    }

    [Fact]
    public async Task CreateBorrowingRequestAsync_BookNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var requestDto = new CreateBorrowingRequestDto { BookIds = new List<Guid> { bookId } };
        _createValidatorMock.Setup(x => x.ValidateAsync(requestDto, default))
            .ReturnsAsync(new ValidationResult());
        _requestRepositoryMock.Setup(x => x.GetRequestInMonth(userId, It.IsAny<int>()))
            .ReturnsAsync(0);
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateBorrowingRequestAsync(requestDto, userId));
    }

    [Fact]
    public async Task CreateBorrowingRequestAsync_BookNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var requestDto = new CreateBorrowingRequestDto { BookIds = new List<Guid> { bookId } };
        var book = new Book { Id = bookId, Title = "Book1", Quantity = 0, Available = 0 };
        _createValidatorMock.Setup(x => x.ValidateAsync(requestDto, default))
            .ReturnsAsync(new ValidationResult());
        _requestRepositoryMock.Setup(x => x.GetRequestInMonth(userId, It.IsAny<int>()))
            .ReturnsAsync(0);
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync(book);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateBorrowingRequestAsync(requestDto, userId));
    }

    [Fact]
    public async Task GetUserBorrowingRequestsAsync_ReturnsUserRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = new List<BookBorrowingRequest>
        {
            new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                RequestorId = userId,
                Details = new List<BookBorrowingRequestDetails>
                {
                    new BookBorrowingRequestDetails { Book = new Book() }
                },
                Requestor = new User()
            }
        };
        var requestDtos = requests.Select(r => new BorrowingRequestDto { Id = r.Id }).ToList();
        var mockQueryable = requests.AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<BorrowingRequestDto>>(requests)).Returns(requestDtos);

        // Act
        var result = await _service.GetUserBorrowingRequestsAsync(userId);

        // Assert
        Assert.Equal(requestDtos, result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllBorrowingRequestsAsync_ReturnsAllRequests()
    {
        // Arrange
        var requests = new List<BookBorrowingRequest>
        {
            new BookBorrowingRequest
            {
                Id = Guid.NewGuid(),
                Details = new List<BookBorrowingRequestDetails>
                {
                    new BookBorrowingRequestDetails { Book = new Book() }
                },
                Requestor = new User()
            }
        };
        var requestDtos = requests.Select(r => new BorrowingRequestDto { Id = r.Id }).ToList();
        var mockQueryable = requests.AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _mapperMock.Setup(x => x.Map<List<BorrowingRequestDto>>(requests)).Returns(requestDtos);

        // Act
        var result = await _service.GetAllBorrowingRequestsAsync();

        // Assert
        Assert.Equal(requestDtos, result);
        Assert.Single(result);
    }

    [Fact]
    public async Task ApproveBorrowingRequestAsync_ValidRequest_ApprovesRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting
        };
        _requestRepositoryMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(request);
        _requestRepositoryMock.Setup(x => x.Update(request)).Verifiable();
        _requestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.ApproveBorrowingRequestAsync(requestId, approverId);

        // Assert
        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.Equal(approverId, request.ApprovedId);
        _requestRepositoryMock.Verify(x => x.Update(request), Times.Once());
        _requestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task ApproveBorrowingRequestAsync_InvalidRequest_ThrowsKeyNotFoundException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        _requestRepositoryMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync((BookBorrowingRequest)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.ApproveBorrowingRequestAsync(requestId, approverId));
    }

    [Fact]
    public async Task ApproveBorrowingRequestAsync_AlreadyApproved_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Approved
        };
        _requestRepositoryMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(request);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ApproveBorrowingRequestAsync(requestId, approverId));
    }

    [Fact]
    public async Task RejectBorrowingRequestAsync_ValidRequest_RejectsRequestAndRestoresAvailability()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var book = new Book { Id = Guid.NewGuid(), Title = "Book1", Available = 0 };
        var request = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting,
            Details = new List<BookBorrowingRequestDetails>
            {
                new BookBorrowingRequestDetails { BookId = book.Id, Book = book }
            }
        };
        var mockQueryable = new List<BookBorrowingRequest> { request }.AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _bookRepositoryMock.Setup(x => x.Update(book)).Verifiable();
        _requestRepositoryMock.Setup(x => x.Update(request)).Verifiable();
        _requestRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.RejectBorrowingRequestAsync(requestId, approverId);

        // Assert
        Assert.Equal(RequestStatus.Rejected, request.Status);
        Assert.Equal(approverId, request.ApprovedId);
        Assert.Equal(1, book.Available); // Availability should increment
        _bookRepositoryMock.Verify(x => x.Update(book), Times.Once());
        _requestRepositoryMock.Verify(x => x.Update(request), Times.Once());
        _requestRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once());
    }

    [Fact]
    public async Task RejectBorrowingRequestAsync_InvalidRequest_ThrowsKeyNotFoundException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var mockQueryable = new List<BookBorrowingRequest>().AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.RejectBorrowingRequestAsync(requestId, approverId));
    }

    [Fact]
    public async Task RejectBorrowingRequestAsync_AlreadyRejected_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var request = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Rejected,
            Details = new List<BookBorrowingRequestDetails>()
        };
        var mockQueryable = new List<BookBorrowingRequest> { request }.AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RejectBorrowingRequestAsync(requestId, approverId));
    }

    [Fact]
    public async Task RejectBorrowingRequestAsync_BookNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var request = new BookBorrowingRequest
        {
            Id = requestId,
            Status = RequestStatus.Waiting,
            Details = new List<BookBorrowingRequestDetails>
            {
                new BookBorrowingRequestDetails { BookId = bookId, Book = null }
            }
        };
        var mockQueryable = new List<BookBorrowingRequest> { request }.AsQueryable().BuildMockDbSet();
        _requestRepositoryMock.Setup(x => x.Query()).Returns(mockQueryable.Object);
        _bookRepositoryMock.Setup(x => x.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.RejectBorrowingRequestAsync(requestId, approverId));
    }
}