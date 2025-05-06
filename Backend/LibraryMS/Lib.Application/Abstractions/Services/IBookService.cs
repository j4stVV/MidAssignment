using Lib.Application.Dtos;
using Lib.Application.Dtos.BookDTOs;

namespace Lib.Application.Abstractions.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto> GetBookByIdAsync(Guid id);
    Task CreateBookAsync(CreateBookDto dto);
    Task UpdateBookAsync(UpdateBookDto dto);
    Task DeleteBookAsync(Guid id);
    Task<PagedResultDto<BookDto>> GetBooksAsync(int pageNumber, int pageSize = 10);
    Task<PagedResultDto<BookDto>> GetFilteredBooksAsync(
    string? title = null,
    string? author = null,
    Guid? categoryId = null,
    bool? available = null,
    int pageNumber = 1,
    int pageSize = 5);
}
