using Lib.Domain.Entities;
using Lib.Infrastructure.Services;
using Lib.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryMSTests.Services;
public class RefreshTokenServiceTests
{
    private readonly AppDbContext _context;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new AppDbContext(options);
        _refreshTokenService = new RefreshTokenService(_context);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldGenerateAndSaveToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _refreshTokenService.GenerateRefreshTokenAsync(userId, ipAddress);

        // Assert
        Assert.NotNull(result);
        var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId);
        Assert.NotNull(savedToken);
        Assert.Equal(result, savedToken.Token);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnToken_WhenValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser"
        };

        var token = "valid-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            IsRevoked = false,
            Expires = DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow,
            UserId = user.Id,
            User = user
        };

        // Add the user and token to the in-memory database
        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _refreshTokenService.ValidateRefreshTokenAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.Token);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldRevokeToken_WhenTokenExists()
    {
        // Arrange
        var token = "valid-token";
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            IsRevoked = false,
            Expires = DateTime.UtcNow.AddDays(1),
            Created = DateTime.UtcNow,
            UserId = Guid.NewGuid()
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        await _refreshTokenService.RevokeRefreshTokenAsync(token, "127.0.0.1");

        // Assert
        var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        Assert.NotNull(revokedToken);
        Assert.True(revokedToken.IsRevoked);
    }
}