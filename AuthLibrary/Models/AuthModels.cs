namespace AuthLibrary.Models;

/// <summary>
/// User entity tương ứng với bảng auth_user
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Password { get; set; } = string.Empty;
    public DateTimeOffset? LastLogin { get; set; }
    public bool IsSuperuser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsStaff { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset DateJoined { get; set; }

    /// <summary>
    /// Full name for display
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Token entity tương ứng với bảng auth_token
/// </summary>
public class AuthToken
{
    public long Id { get; set; }
    public string TokenKey { get; set; } = string.Empty;
    public int IdUser { get; set; }
    public DateTime? ExpireDate { get; set; }

    /// <summary>
    /// Check if token is still valid
    /// </summary>
    public bool IsValid => ExpireDate.HasValue && ExpireDate.Value > DateTime.UtcNow;
}
