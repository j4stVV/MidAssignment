using Lib.Application.Dtos.AuthDto;
using Lib.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Lib.Application.Abstractions.Services;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage)> RegisterAsync(string loginId, string password);
    Task<(bool Success, string AccessToken, string RefreshToken, string ErrorMessage)> LoginAsync(string loginId, string password, string ipAddress);
    Task<(bool Success, string AccessToken, string RefreshToken, string ErrorMessage)> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<(bool Success, string ErrorMessage)> LogoutAsync(string refreshToken, string ipAddress);
    Task<string> GetUserRoleAsync(Guid userId);
    Task<(bool Success, UserProfileDto Profile, string ErrorMessage)>
            GetProfileAsync(string userId);
}
