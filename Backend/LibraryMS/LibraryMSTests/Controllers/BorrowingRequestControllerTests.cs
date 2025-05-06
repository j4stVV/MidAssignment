using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.Borrowing;
using LibraryManament.Controller;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace LibraryMSTests.Controllers;
public class BorrowingRequestControllerTests
{
    private readonly Mock<IBookBorrowingRequestService> _mockService;
    private readonly BorrowingRequestController _controller;
    private readonly Guid _testUserId;

    public BorrowingRequestControllerTests()
    {
        _mockService = new Mock<IBookBorrowingRequestService>();
        _controller = new BorrowingRequestController(_mockService.Object);

        _testUserId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "User") // Simulate role for attribute logic
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateBorrowingRequest_ReturnsOk()
    {
        var requestDto = new CreateBorrowingRequestDto
        {
            BookIds = new List<Guid> { Guid.NewGuid() }
        };

        _mockService.Setup(x => x.CreateBorrowingRequestAsync(requestDto, _testUserId))
                    .ReturnsAsync(new BorrowingRequestDto());

        var result = await _controller.CreateBorrowingRequest(requestDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Borrowing request created successfully.",
            okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task GetMyBorrowingRequests_ReturnsList()
    {
        var expected = new List<BorrowingRequestDto>
        {
            new BorrowingRequestDto { Id = Guid.NewGuid() }
        };

        _mockService.Setup(x => x.GetUserBorrowingRequestsAsync(_testUserId))
                    .ReturnsAsync(expected);

        var result = await _controller.GetMyBorrowingRequests();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task GetAllBorrowingRequests_ReturnsList()
    {
        // Switch to SuperUser
        SetUserRole("SuperUser");

        var expected = new List<BorrowingRequestDto> { new() { Id = Guid.NewGuid() } };
        _mockService.Setup(x => x.GetAllBorrowingRequestsAsync()).ReturnsAsync(expected);

        var result = await _controller.GetAllBorrowingRequests();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ApproveBorrowingRequest_ReturnsOk()
    {
        // Switch to SuperUser
        SetUserRole("SuperUser");

        var requestId = Guid.NewGuid();
        _mockService.Setup(x => x.ApproveBorrowingRequestAsync(requestId, _testUserId))
                    .Returns(Task.CompletedTask);

        var result = await _controller.ApproveBorrowingRequest(requestId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Borrowing request approved successfully.",
            okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task RejectBorrowingRequest_ReturnsOk()
    {
        // Switch to SuperUser
        SetUserRole("SuperUser");

        var requestId = Guid.NewGuid();
        _mockService.Setup(x => x.RejectBorrowingRequestAsync(requestId, _testUserId))
                    .Returns(Task.CompletedTask);

        var result = await _controller.RejectBorrowingRequest(requestId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Borrowing request rejected successfully.",
            okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value));
    }

    private void SetUserRole(string role)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}