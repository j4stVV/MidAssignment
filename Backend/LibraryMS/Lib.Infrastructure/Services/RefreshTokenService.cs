using Lib.Application.Abstractions.Services;
using Lib.Domain.Entities;
using Lib.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Lib.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    public RefreshTokenService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(randomBytes);

        var existingToken = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        foreach (var token in existingToken)
        {
            token.IsRevoked = true;
        }

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
        {
            throw new Exception("Invalid Refresh Token");
        }

        if (refreshToken.IsRevoked || refreshToken.Expires < DateTime.UtcNow)
        {
            throw new Exception("Refresh Token is expired or revoked");
        }

        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }
}
