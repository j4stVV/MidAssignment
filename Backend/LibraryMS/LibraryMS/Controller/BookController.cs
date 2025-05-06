using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.BookDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManament.Controller;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("all")]
    [Authorize(Roles = "User,SuperUser")]
    public async Task<IActionResult> GetBooks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _bookService.GetBooksAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Create([FromBody] CreateBookDto bookDto)
    {
        await _bookService.CreateBookAsync(bookDto);
        return Ok(new { Message = "Book created successfully." });
    }

    [HttpPut]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Update([FromBody] UpdateBookDto bookDto)
    {
        await _bookService.UpdateBookAsync(bookDto);
        return Ok(new { Message = "Book updated successfully." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bookService.DeleteBookAsync(id);
        return Ok(new { Message = "Book deleted successfully." });
    }

    [HttpGet("filter")]
    [Authorize(Roles = "User,SuperUser")]
    public async Task<IActionResult> GetFilteredBooks(
        [FromQuery] string? title = null,
        [FromQuery] string? author = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? available = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var result = await _bookService.GetFilteredBooksAsync(title, author, categoryId, available, pageNumber, pageSize);
        return Ok(result);
    }
}
