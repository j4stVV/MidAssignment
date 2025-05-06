using AutoMapper;
using FluentValidation;
using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.Borrowing;
using Lib.Domain.Entities;
using Lib.Domain.Enum;
using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Infrastructure.Services;

public class BookBorrowingRequestService : IBookBorrowingRequestService
{
    private readonly IBookBorrowingRequestRepository _requestRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateBorrowingRequestDto> _createValidator;

    public BookBorrowingRequestService(
        IBookBorrowingRequestRepository requestRepository,
        IBookRepository bookRepository,
        IMapper mapper,
        IValidator<CreateBorrowingRequestDto> createValidator)
    {
        _requestRepository = requestRepository;
        _bookRepository = bookRepository;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<BorrowingRequestDto> CreateBorrowingRequestAsync(CreateBorrowingRequestDto requestDto, Guid userId)
    {
        var validationResult = await _createValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userRequestsThisMonth = await _requestRepository.GetRequestInMonth(userId, DateTime.UtcNow.Month);
        if (userRequestsThisMonth >= 3)
            throw new InvalidOperationException("Exceeded monthly borrowing request limit of 3.");

        var requestDetails = new List<BookBorrowingRequestDetails>();
        foreach (var bookId in requestDto.BookIds)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException($"Book with ID {bookId} not found.");
            if (book.Quantity <= 0)
                throw new InvalidOperationException($"Book {book.Title} is not available.");

            book.Available--;

            requestDetails.Add(new BookBorrowingRequestDetails
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                Book = book
            });
        }

        var request = new BookBorrowingRequest
        {
            Id = Guid.NewGuid(),
            RequestorId = userId,
            RequestedDate = DateTime.UtcNow,
            Status = RequestStatus.Waiting,
            Details = requestDetails
        };

        await _requestRepository.AddAsync(request);

        foreach (var detail in request.Details)
        {
            _bookRepository.Update(detail.Book);
        }

        await _requestRepository.SaveChangesAsync();

        return _mapper.Map<BorrowingRequestDto>(request);
    }

    public async Task<List<BorrowingRequestDto>> GetUserBorrowingRequestsAsync(Guid userId)
    {
        var userRequests = await _requestRepository.Query()
            .Where(r => r.RequestorId == userId)
            .Include(r => r.Details)
            .ThenInclude(d => d.Book)
            .Include(r => r.Requestor)
            .ToListAsync();

        return _mapper.Map<List<BorrowingRequestDto>>(userRequests);
    }

    public async Task<List<BorrowingRequestDto>> GetAllBorrowingRequestsAsync()
    {
        var allRequests = await _requestRepository.Query()
            .Include(r => r.Details)
            .ThenInclude(d => d.Book)
            .Include(n => n.Requestor)
            .ToListAsync();

        return _mapper.Map<List<BorrowingRequestDto>>(allRequests);
    }
    public async Task ApproveBorrowingRequestAsync(Guid requestId, Guid approverId)
    {
        var request = await _requestRepository.GetByIdAsync(requestId);
        if (request == null) throw new KeyNotFoundException("Request not found.");
        if (request.Status == RequestStatus.Approved)
            throw new InvalidOperationException("Request is already approved.");
        if (request.Status == RequestStatus.Rejected)
            throw new InvalidOperationException("Rejected request cannot be approved.");    
        request.Status = RequestStatus.Approved;
        request.ApprovedId = approverId;
        _requestRepository.Update(request);
        await _requestRepository.SaveChangesAsync();
    }

    public async Task RejectBorrowingRequestAsync(Guid requestId, Guid approverId)
    {
        var request = await _requestRepository.Query()
            .Include(r => r.Details)
            .ThenInclude(d => d.Book)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("Borrowing Request not found.");

        if (request.Status == RequestStatus.Rejected)
            throw new InvalidOperationException("Request is already rejected.");

        if (request.Status == RequestStatus.Approved)
            throw new InvalidOperationException("Approved request cannot be rejected.");

        request.Status = RequestStatus.Rejected;
        request.ApprovedId = approverId;

        if (request.Status == RequestStatus.Rejected)
        {
            foreach (var detail in request.Details)
            {
                var book = detail.Book ?? await _bookRepository.GetByIdAsync(detail.BookId);
                if (book == null)
                    throw new KeyNotFoundException($"Book with ID {detail.BookId} not found.");

                book.Available++;
                _bookRepository.Update(book);
            }
        }
        
        _requestRepository.Update(request);

        await _requestRepository.SaveChangesAsync();
    }
}

