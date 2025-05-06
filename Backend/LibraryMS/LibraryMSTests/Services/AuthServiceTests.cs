using Lib.Application.Abstractions.Services;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Lib.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryMSTests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;
    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _configMock = new Mock<IConfiguration>();

        // Setup configuration with distinct mocks for each section
        var keySection = new Mock<IConfigurationSection>();
        keySection.Setup(x => x.Value).Returns("SuperSecretKey123456789012345678901234567890"); // 40 chars > 32 bytes
        _configMock.Setup(x => x.GetSection("Jwt:Key")).Returns(keySection.Object);

        var issuerSection = new Mock<IConfigurationSection>();
        issuerSection.Setup(x => x.Value).Returns("https://issuer.com");
        _configMock.Setup(x => x.GetSection("Jwt:Issuer")).Returns(issuerSection.Object);

        var audienceSection = new Mock<IConfigurationSection>();
        audienceSection.Setup(x => x.Value).Returns("https://audience.com");
        _configMock.Setup(x => x.GetSection("Jwt:Audience")).Returns(audienceSection.Object);

        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _refreshTokenServiceMock.Object,
            _configMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var loginId = "testuser";
        var password = "Password123!";
        _userManagerMock.Setup(x => x.FindByNameAsync(loginId)).ReturnsAsync((User)null);
        _userManagerMock.Setup(x => x.FindByEmailAsync(loginId)).ReturnsAsync((User)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(loginId, password);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_EmptyLoginId_ThrowsRegisterValidationException()
    {
        // Arrange
        var loginId = "";
        var password = "Password123!";

        // Act & Assert
        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            _authService.RegisterAsync(loginId, password));
    }

    [Fact]
    public async Task RegisterAsync_ExistingUsername_ThrowsRegisterValidationException()
    {
        // Arrange
        var loginId = "testuser";
        var password = "Password123!";
        var user = new User { UserName = loginId };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginId)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<RegisterValidationException>(() =>
            _authService.RegisterAsync(loginId, password));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var loginId = "testuser";
        var password = "Password123!";
        var ipAddress = "127.0.0.1";
        var user = new User { Id = Guid.NewGuid(), UserName = loginId };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginId)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(user.Id, ipAddress))
            .ReturnsAsync("refreshToken");

        // Act
        var result = await _authService.LoginAsync(loginId, password, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("refreshToken", result.RefreshToken);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsInvalidOperationException()
    {
        // Arrange
        var loginId = "testuser";
        var password = "WrongPassword";
        var ipAddress = "127.0.0.1";
        var user = new User { Id = Guid.NewGuid(), UserName = loginId };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginId)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.LoginAsync(loginId, password, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshToken = "validRefreshToken";
        var ipAddress = "127.0.0.1";
        var user = new User { Id = Guid.NewGuid(), UserName = "testuser" };
        var token = new RefreshToken { User = user };
        _refreshTokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
            .ReturnsAsync(token);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
        _refreshTokenServiceMock.Setup(x => x.GenerateRefreshTokenAsync(user.Id, ipAddress))
            .ReturnsAsync("newRefreshToken");

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("newRefreshToken", result.RefreshToken);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_ReturnsSuccess()
    {
        // Arrange
        var refreshToken = "validRefreshToken";
        var ipAddress = "127.0.0.1";
        _refreshTokenServiceMock.Setup(x => x.RevokeRefreshTokenAsync(refreshToken, ipAddress))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync(refreshToken, ipAddress);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserRoleAsync_ValidUserId_ReturnsRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        // Act
        var role = await _authService.GetUserRoleAsync(userId);

        // Assert
        Assert.Equal("Admin", role);
    }

    [Fact]
    public async Task GetUserRoleAsync_InvalidUserId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.GetUserRoleAsync(userId));
    }

    [Fact]
    public async Task GetProfileAsync_ValidUserId_ReturnsProfile()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = Guid.Parse(userId), UserName = "testuser", Email = "test@example.com" };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.GetProfileAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(userId, result.Profile.Id);
        Assert.Equal("testuser", result.Profile.Name);
        Assert.Equal("test@example.com", result.Profile.Email);
        Assert.Equal("User", result.Profile.Role);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public async Task GetProfileAsync_InvalidUserId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.GetProfileAsync(userId));
    }
}