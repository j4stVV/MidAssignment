using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.AuthDto;
using Lib.Domain.Entities;
using Lib.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Lib.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<User> userManager,  
        SignInManager<User> signInManager,
        IRefreshTokenService refreshTokenService,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _refreshTokenService = refreshTokenService;
        _config = config;
    }
    public async Task<(bool Success, string ErrorMessage)>
        RegisterAsync(string loginId, string password)
    {
        if (string.IsNullOrWhiteSpace(loginId) || string.IsNullOrWhiteSpace(password))
        {
            throw new RegisterValidationException("Validation Error",["Username is required."]);
        }
        var existingUserByUsername = await _userManager.FindByNameAsync(loginId);
        if (existingUserByUsername != null)
        {
            throw new RegisterValidationException("Validation Error",["Username is already taken."]);
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(loginId);
        if (existingUserByEmail != null)
        {
            throw new RegisterValidationException("Validation Error", ["Email is already taken."]);
        }

        var user = new User
        {
            UserName = loginId,
            NormalizedUserName = loginId.ToUpper()
        };

        if (IsValidEmail(loginId))
        {
            user.Email = loginId;
            user.NormalizedEmail = loginId.ToUpper();
        }

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new RegisterValidationException("Registration failed.", [errors]);
        }

        await _userManager.AddToRoleAsync(user, "User");
        return (true, string.Empty);
    }
    public async Task<(bool Success, string AccessToken, string RefreshToken, string ErrorMessage)> 
        LoginAsync(string loginId, string password, string ipAddress)
    {
        var user = await _userManager.FindByNameAsync(loginId);
        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(loginId);
        }
        if (user == null)
        {
            throw new InvalidOperationException("Invalid Username or Email or Password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Invalid Username or Email or Password.");
        }

        var accessToken = await GenerateJwtToken(user);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);
        return (true, accessToken, refreshToken, string.Empty);
    }

    public async Task<(bool Success, string AccessToken, string RefreshToken, string ErrorMessage)> 
        RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
        var user = token.User;

        var newAccessToken = await GenerateJwtToken(user);

        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

        return (true, newAccessToken, newRefreshToken, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> 
        LogoutAsync(string refreshToken, string ipAddress)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);
        return (true, string.Empty);
    }

    public async Task<string> GetUserRoleAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? string.Empty;
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes
            (_config.GetSection("Jwt:Key").Value));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(15);

        var token = new JwtSecurityToken(
            issuer: _config.GetSection("Jwt:Issuer").Value,
            audience: _config.GetSection("Jwt:Audience").Value,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public async Task<(bool Success, UserProfileDto Profile, string ErrorMessage)>
            GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var profile = new UserProfileDto
        {
            Id = user.Id.ToString(), // Guid as string
            Name = user.UserName!,
            Email = user.Email!,
            Role = GetUserRoleAsync(user.Id).Result
        };

        return (true, profile, string.Empty);
    }
}
