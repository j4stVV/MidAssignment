using AutoMapper;
using FluentValidation;
using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos;
using Lib.Application.Dtos.BookDTOs;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<Book> _validator;
    private readonly IValidator<CreateBookDto> _createValidator;
    private readonly IValidator<UpdateBookDto> _updateValidator;

    public BookService(
        IBookRepository bookRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IValidator<Book> validator,
        IValidator<CreateBookDto> createValidator,
        IValidator<UpdateBookDto> updateValidator)
    {
        _bookRepository = bookRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _validator = validator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.Query()
                .Include(b => b.Category)
                .ToListAsync();
        return _mapper.Map<List<BookDto>>(books);
    }

    public async Task<PagedResultDto<BookDto>> GetBooksAsync(int pageNumber, int pageSize = 5)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.");
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0.");

        var query = await _bookRepository.Query()
            .Include(b => b.Category)
            .ToListAsync();

        var totalItems = query.Count();
        var items = query
                .OrderBy(b => b.Title) // Sort by title
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

        return new PagedResultDto<BookDto>
        {
            Items = _mapper.Map<List<BookDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
    public async Task<BookDto> GetBookByIdAsync(Guid id)
    {
        var book = await _bookRepository.Query()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
            throw new KeyNotFoundException($"Book with ID {id} not found.");
        return _mapper.Map<BookDto>(book);
    }

    public async Task CreateBookAsync(CreateBookDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _categoryRepository.GetByIdAsync(dto.CategoryId) == null)
            throw new KeyNotFoundException($"Category with ID {dto.CategoryId} not found.");

        var book = _mapper.Map<Book>(dto);
        await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(UpdateBookDto dto)
    {

        var book = await _bookRepository.GetByIdAsync(dto.Id);
        if (book == null)
            throw new KeyNotFoundException($"Book with ID {dto.Id} not found.");

        dto.Available = (dto.Quantity - book.Quantity) + book.Available;

        if (dto.Available < 0) dto.Available = 0;

        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _categoryRepository.GetByIdAsync(dto.CategoryId) == null)
            throw new KeyNotFoundException($"Category with ID {dto.CategoryId} not found.");

        _mapper.Map(dto, book);
        _bookRepository.Update(book);
        await _bookRepository.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(Guid id)
    {
        var book = await _bookRepository.Query()
                .Include(b => b.BorrowingDetails)
                .FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
            throw new KeyNotFoundException($"Book with ID {id} not found.");
        if (book.BorrowingDetails.Any())
            throw new BusinessRuleException("Cannot delete book with active borrowing requests.");
        await _bookRepository.DeleteAsync(book);
    }

    public async Task<PagedResultDto<BookDto>> GetFilteredBooksAsync(
    string? title = null,
    string? author = null,
    Guid? categoryId = null,
    bool? available = null,
    int pageNumber = 1,
    int pageSize = 5)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.");
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0.");

        var query = _bookRepository.Query()
            .Include(b => b.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(b => b.Title.ToLower().Contains(title.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId.Value);
        }

        if (available.HasValue)
        {
            query = query.Where(b => b.Available > 0 == available.Value);
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Title) 
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<BookDto>
        {
            Items = _mapper.Map<List<BookDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
