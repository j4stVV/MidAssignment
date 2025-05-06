using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.AuthDto;
using LibraryManament.Controller;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Moq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace LibraryMSTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var dto = new RegisterDto { LoginId = "testuser", Password = "Password123" };
        _mockAuthService.Setup(x => x.RegisterAsync(dto.LoginId, dto.Password))
            .ReturnsAsync((true, string.Empty));

        // Act
        var result = await _controller.Register(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenFailed()
    {
        var dto = new RegisterDto { LoginId = "testuser", Password = "pass" };
        _mockAuthService.Setup(x => x.RegisterAsync(dto.LoginId, dto.Password))
            .ReturnsAsync((false, "Validation failed"));

        var result = await _controller.Register(dto);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badResult.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithTokens()
    {
        var dto = new LoginDto { LoginId = "user", Password = "pass" };
        _mockAuthService.Setup(x => x.LoginAsync(dto.LoginId, dto.Password, It.IsAny<string>()))
            .ReturnsAsync((true, "access-token", "refresh-token", string.Empty));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            }
        };

        var result = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        // Serialize the object to JSON and then deserialize it into a Dictionary<string, string>
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        Assert.NotNull(response);
        Assert.Equal("access-token", response["AccessToken"]);
        Assert.Equal("refresh-token", response["RefreshToken"]);
    }


    [Fact]
    public async Task Login_ReturnsBadRequest_WhenFailed()
    {
        // Arrange
        var dto = new LoginDto { LoginId = "user", Password = "wrong" };
        _mockAuthService.Setup(x => x.LoginAsync(dto.LoginId, dto.Password, It.IsAny<string>()))
            .ReturnsAsync((false, null, null, "Invalid credentials"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = await _controller.Login(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReturnsOk_WhenValid()
    {
        var dto = new RefreshTokenDto { RefreshToken = "valid-token" };
        _mockAuthService.Setup(x => x.RefreshTokenAsync(dto.RefreshToken, It.IsAny<string>()))
            .ReturnsAsync((true, "new-access", "new-refresh", string.Empty));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            }
        };

        var result = await _controller.Refresh(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        // Serialize the anonymous object and then deserialize it into a Dictionary<string, string>
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        Assert.NotNull(response);
        Assert.Equal("new-access", response["AccessToken"]);
        Assert.Equal("new-refresh", response["RefreshToken"]);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenInvalid()
    {
        var dto = new RefreshTokenDto { RefreshToken = "invalid" };
        _mockAuthService.Setup(x => x.RefreshTokenAsync(dto.RefreshToken, It.IsAny<string>()))
            .ReturnsAsync((false, null, null, "Invalid"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            }
        };

        var result = await _controller.Refresh(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Logout_ReturnsOk_WhenSuccess()
    {
        var dto = new RefreshTokenDto { RefreshToken = "token" };
        _mockAuthService.Setup(x => x.LogoutAsync(dto.RefreshToken, It.IsAny<string>()))
            .ReturnsAsync((true, string.Empty));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            }
        };

        var result = await _controller.Logout(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Logout_ReturnsBadRequest_WhenFailed()
    {
        var dto = new RefreshTokenDto { RefreshToken = "token" };
        _mockAuthService.Setup(x => x.LogoutAsync(dto.RefreshToken, It.IsAny<string>()))
            .ReturnsAsync((false, "Error"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
            {
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") }
            }
        };

        var result = await _controller.Logout(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenFound()
    {
        var userId = "user-id";
        var profile = new UserProfileDto { Id = userId, Name = "Test", Email = "test@mail.com", Role = "User" };

        _mockAuthService.Setup(x => x.GetProfileAsync(userId))
            .ReturnsAsync((true, profile, string.Empty));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }))
            }
        };

        var result = await _controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returnedProfile = Assert.IsType<UserProfileDto>(ok.Value);
        Assert.Equal(userId, returnedProfile.Id);
    }

    [Fact]
    public async Task GetProfile_ReturnsUnauthorized_WhenNoClaim()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await _controller.GetProfile();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}