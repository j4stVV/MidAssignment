using Lib.Domain.Entities;

namespace Lib.Application.Abstractions.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
    Task<RefreshToken> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string ipAddress);
}
