using AuthLibrary.DTOs;
using AuthLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataProcessingAPI.Controllers.Base;

namespace DataProcessingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
        : base(logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        return await ExecuteAsync(async () =>
        {
            var result = await _authService.LoginAsync(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("User {Username} logged in successfully", request.Username);
                return result.Data!;
            }

            _logger.LogWarning("Login failed for user {Username}: {Error}", request.Username, result.ErrorMessage);
            throw new UnauthorizedAccessException(result.ErrorMessage ?? "Invalid credentials");
        }, "user login", "Login successful");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        return await ExecuteAsync(async () =>
        {
            var result = await _authService.RegisterAsync(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("User {Username} registered successfully", request.Username);
                return result.Data!;
            }

            _logger.LogWarning("Registration failed for user {Username}: {Error}", request.Username, result.ErrorMessage);
            throw new ArgumentException(result.ErrorMessage ?? "Registration failed");
        }, "user registration", "Registration successful", 201);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        return await ExecuteAsync(async () =>
        {
            var userId = GetCurrentUserId();
            var result = await _authService.ChangePasswordAsync(userId, request);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return result.Data!;
            }

            _logger.LogWarning("Password change failed for user {UserId}: {Error}", userId, result.ErrorMessage);
            throw new ArgumentException(result.ErrorMessage ?? "Password change failed");
        }, "password change", "Password changed successfully");
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        return await ExecuteAsync(async () =>
        {
            var token = GetAuthToken();
            var result = await _authService.ValidateTokenAsync(token);
            
            if (result.IsSuccess)
            {
                return result.Data!;
            }

            throw new UnauthorizedAccessException(result.ErrorMessage ?? "Invalid token");
        }, "get profile", "Profile retrieved successfully");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        return await ExecuteAsync(async () =>
        {
            var token = GetAuthToken();
            var result = await _authService.LogoutAsync(token);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("User logged out successfully");
                return result.Data!;
            }

            throw new ArgumentException(result.ErrorMessage ?? "Logout failed");
        }, "logout", "Logout successful");
    }
}