using AuthLibrary.DTOs;
using AuthLibrary.Models;

namespace AuthLibrary.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user with username/password
    /// </summary>
    Task<AuthResult> LoginAsync(LoginRequest request);

    /// <summary>
    /// Register new user
    /// </summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Validate token and get user info
    /// </summary>
    Task<AuthResult> ValidateTokenAsync(string token);

    /// <summary>
    /// Logout user (invalidate token)
    /// </summary>
    Task<AuthResult> LogoutAsync(string token);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}

/// <summary>
/// Token service interface
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    bool ValidateToken(string token, out int userId);

    /// <summary>
    /// Get token expiry date
    /// </summary>
    DateTime GetTokenExpiry();
}

/// <summary>
/// Password service interface
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hash password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
