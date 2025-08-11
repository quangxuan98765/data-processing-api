using AuthLibrary.Interfaces;
using BCrypt.Net;

namespace AuthLibrary.Services;

/// <summary>
/// Password service using BCrypt for secure hashing
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor for security

    /// <summary>
    /// Hash password using BCrypt
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verify password against BCrypt hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
