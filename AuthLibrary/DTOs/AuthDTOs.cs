namespace AuthLibrary.DTOs;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
}

/// <summary>
/// User info DTO (không chứa password)
/// </summary>
public class UserInfo
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsStaff { get; set; }
    public bool IsSuperuser { get; set; }
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Register request DTO
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Auth result DTO
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static AuthResult Success(object? data = null)
    {
        return new AuthResult { IsSuccess = true, Data = data };
    }

    public static AuthResult Failed(string message)
    {
        return new AuthResult { IsSuccess = false, ErrorMessage = message };
    }
}
