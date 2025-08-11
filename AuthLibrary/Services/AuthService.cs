using AuthLibrary.DTOs;
using AuthLibrary.Interfaces;
using AuthLibrary.Models;
using DataAccess;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AuthLibrary.Services;

/// <summary>
/// Authentication service implementation - Simplified
/// </summary>
public class AuthService : IAuthService
{
    private readonly IDatabaseService _databaseService;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(
        IDatabaseService databaseService, 
        IPasswordService passwordService, 
        ITokenService tokenService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            // Find user by username
            var user = await GetUserByUsernameAsync(request.Username);
            if (user == null)
            {
                return AuthResult.Failed("Invalid username or password");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.Password))
            {
                return AuthResult.Failed("Invalid username or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return AuthResult.Failed("Account is disabled");
            }

            // Generate JWT token
            var token = _tokenService.GenerateToken(user);
            var expiry = _tokenService.GetTokenExpiry();

            // Store token in database
            await StoreTokenAsync(user.Id, token, expiry);

            var response = new LoginResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = expiry,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsStaff = user.IsStaff,
                    IsSuperuser = user.IsSuperuser
                }
            };

            return AuthResult.Success(response);
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Authentication error: {ex.Message}");
        }
    }

    /// <summary>
    /// Register new user
    /// </summary>
    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUser = await GetUserByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return AuthResult.Failed("Username already exists");
            }

            // Check if email already exists
            existingUser = await GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return AuthResult.Failed("Email already exists");
            }

            // Hash password
            var hashedPassword = _passwordService.HashPassword(request.Password);

            // Create user using ExecuteStoredProcAsync
            var parameters = new
            {
                Username = request.Username,
                Password = hashedPassword,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsStaff = false,
                IsSuperuser = false,
                IsActive = true,
                DateJoined = DateTime.UtcNow
            };

            var result = await _databaseService.ExecuteStoredProcAsync("sp_CreateUser", parameters);
            
            if (result.Rows.Count > 0)
            {
                var userId = Convert.ToInt32(result.Rows[0]["UserId"]);
                var newUser = await GetUserByIdAsync(userId);
                
                if (newUser != null)
                {
                    var userInfo = new UserInfo
                    {
                        Id = newUser.Id,
                        Username = newUser.Username,
                        Email = newUser.Email,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        IsStaff = newUser.IsStaff,
                        IsSuperuser = newUser.IsSuperuser
                    };

                    return AuthResult.Success(userInfo);
                }
            }

            return AuthResult.Failed("Failed to create user");
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Registration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return AuthResult.Failed("User not found");
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.Password))
            {
                return AuthResult.Failed("Current password is incorrect");
            }

            // Hash new password
            var hashedPassword = _passwordService.HashPassword(request.NewPassword);

            // Update password
            var parameters = new
            {
                UserId = userId,
                NewPassword = hashedPassword
            };

            var result = await _databaseService.ExecuteStoredProcAsync("sp_ChangePassword", parameters);

            if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0]["RowsAffected"]) > 0)
            {
                // Invalidate all existing tokens for this user
                await InvalidateUserTokensAsync(userId);
                return AuthResult.Success("Password changed successfully");
            }

            return AuthResult.Failed("Failed to change password");
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Change password error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate authentication token
    /// </summary>
    public async Task<AuthResult> ValidateTokenAsync(string token)
    {
        try
        {
            // Validate JWT token
            if (!_tokenService.ValidateToken(token, out int userId))
            {
                return AuthResult.Failed("Invalid token");
            }

            // Check if token exists in database and is not expired
            var authToken = await GetTokenAsync(token);
            if (authToken == null || authToken.ExpireDate <= DateTime.UtcNow)
            {
                return AuthResult.Failed("Token expired or not found");
            }

            var user = await GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return AuthResult.Failed("User not found or inactive");
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsStaff = user.IsStaff,
                IsSuperuser = user.IsSuperuser
            };

            return AuthResult.Success(userInfo);
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Token validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Logout user and invalidate token
    /// </summary>
    public async Task<AuthResult> LogoutAsync(string token)
    {
        try
        {
            await InvalidateTokenAsync(token);
            return AuthResult.Success("Logged out successfully");
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"Logout error: {ex.Message}");
        }
    }

    // Private helper methods
    private async Task<User?> GetUserByUsernameAsync(string username)
    {
        var parameters = new { Username = username };
        var result = await _databaseService.QueryAsync("SELECT * FROM auth_user WHERE username = @Username", parameters);
        
        if (result.Rows.Count > 0)
        {
            var row = result.Rows[0];
            return MapToUser(row);
        }
        
        return null;
    }

    private async Task<User?> GetUserByEmailAsync(string email)
    {
        var parameters = new { Email = email };
        var result = await _databaseService.QueryAsync("SELECT * FROM auth_user WHERE email = @Email", parameters);
        
        if (result.Rows.Count > 0)
        {
            var row = result.Rows[0];
            return MapToUser(row);
        }
        
        return null;
    }

    private async Task<User?> GetUserByIdAsync(int id)
    {
        var parameters = new { Id = id };
        var result = await _databaseService.QueryAsync("SELECT * FROM auth_user WHERE id = @Id", parameters);
        
        if (result.Rows.Count > 0)
        {
            var row = result.Rows[0];
            return MapToUser(row);
        }
        
        return null;
    }

    private async Task StoreTokenAsync(int userId, string token, DateTime expiry)
    {
        var parameters = new
        {
            TokenKey = token,
            IdUser = userId,
            ExpireDate = expiry,
            CreatedDate = DateTime.UtcNow
        };

        await _databaseService.ExecuteStoredProcAsync("sp_CreateAuthToken", parameters);
    }

    private async Task<AuthToken?> GetTokenAsync(string token)
    {
        var parameters = new { TokenKey = token };
        var result = await _databaseService.QueryAsync("SELECT * FROM auth_token WHERE token_key = @TokenKey", parameters);
        
        if (result.Rows.Count > 0)
        {
            var row = result.Rows[0];
            return new AuthToken
            {
                Id = Convert.ToInt32(row["id"]),
                TokenKey = row["token_key"].ToString() ?? string.Empty,
                IdUser = Convert.ToInt32(row["idUser"]),
                ExpireDate = Convert.ToDateTime(row["expire_date"])
            };
        }
        
        return null;
    }

    private async Task InvalidateTokenAsync(string token)
    {
        var parameters = new { TokenKey = token };
        await _databaseService.ExecuteStoredProcAsync("sp_InvalidateToken", parameters);
    }

    private async Task InvalidateUserTokensAsync(int userId)
    {
        var parameters = new { UserId = userId };
        await _databaseService.ExecuteStoredProcAsync("sp_InvalidateUserTokens", parameters);
    }

    private static User MapToUser(DataRow row)
    {
        return new User
        {
            Id = Convert.ToInt32(row["id"]),
            Username = row["username"].ToString() ?? string.Empty,
            Password = row["password"].ToString() ?? string.Empty,
            Email = row["email"].ToString() ?? string.Empty,
            FirstName = row["first_name"].ToString() ?? string.Empty,
            LastName = row["last_name"].ToString() ?? string.Empty,
            IsStaff = Convert.ToBoolean(row["is_staff"]),
            IsSuperuser = Convert.ToBoolean(row["is_superuser"]),
            IsActive = Convert.ToBoolean(row["is_active"]),
            DateJoined = Convert.ToDateTime(row["date_joined"]),
            LastLogin = row["last_login"] == DBNull.Value ? null : Convert.ToDateTime(row["last_login"])
        };
    }
}
