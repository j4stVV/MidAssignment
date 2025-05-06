using Lib.Application.Abstractions.Services;
using Lib.Application.Dtos.AuthDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryManament.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (success, errorMessage) = await _authService.RegisterAsync(dto.LoginId, dto.Password);
        if (!success)
        {
            return BadRequest(new {Error = errorMessage});
        }
        return Ok(new { Message = "Registed successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, accessToken, refreshToken, errorMessage) = await _authService.LoginAsync(dto.LoginId, dto.Password, ipAddress);
        if (!success)
        {
            return BadRequest(new { Error = errorMessage });
        }
        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, accessToken, refreshToken, errorMessage) = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress);
        if (!success)
        {
            return Unauthorized(new { Error = errorMessage });
        }
        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, errorMessage) = await _authService.LogoutAsync(dto.RefreshToken, ipAddress);
        if (!success)
        {
            return BadRequest(new { Error = errorMessage });
        }
        return Ok(new { Message = "Logged out successfully." });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "Invalid token" });
        }

        var (success, profile, errorMessage) = await _authService.GetProfileAsync(userId);
        if (!success)
        {
            return NotFound(new { Error = errorMessage });
        }

        return Ok(profile);
    }
}
