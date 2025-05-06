using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.Borrowing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManament.Controller;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BorrowingRequestController : ControllerBase
{
    private readonly IBookBorrowingRequestService _borrowingRequestService;

    public BorrowingRequestController(IBookBorrowingRequestService borrowingRequestService)
    {
        _borrowingRequestService = borrowingRequestService;
    }

    [Authorize(Roles = "User")]
    [HttpPost("user/post-request")]
    public async Task<IActionResult> CreateBorrowingRequest([FromBody] CreateBorrowingRequestDto requestDto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found."));
        await _borrowingRequestService.CreateBorrowingRequestAsync(requestDto, userId);
        return Ok(new { Message = "Borrowing request created successfully." });
    }

    [Authorize(Roles = "User")]
    [HttpGet("user/get-request")]
    public async Task<IActionResult> GetMyBorrowingRequests()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found."));
        var requests = await _borrowingRequestService.GetUserBorrowingRequestsAsync(userId);
        return Ok(requests);
    }

    [Authorize(Roles = "SuperUser")]
    [HttpGet("superuser/get-all-request")]
    public async Task<IActionResult> GetAllBorrowingRequests()
    {
        var requests = await _borrowingRequestService.GetAllBorrowingRequestsAsync();
        return Ok(requests);
    }

    [Authorize(Roles = "SuperUser")]
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveBorrowingRequest([FromBody]Guid requestId)
    {
        var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found."));
        await _borrowingRequestService.ApproveBorrowingRequestAsync(requestId, approverId);
        return Ok(new { Message = "Borrowing request approved successfully." });
    }

    [Authorize(Roles = "SuperUser")]
    [HttpPost("reject")]
    public async Task<IActionResult> RejectBorrowingRequest([FromBody]Guid requestId)
    {
        var approverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found."));
        await _borrowingRequestService.RejectBorrowingRequestAsync(requestId, approverId);
        return Ok(new { Message = "Borrowing request rejected successfully." });
    }
}
