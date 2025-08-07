namespace DataProcessingAPI.Shared.Exceptions;

/// <summary>
/// Custom Business Exception
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Data Validation Exception
/// </summary>
public class ValidationException : Exception
{
    public List<string> ValidationErrors { get; }

    public ValidationException(string message) : base(message)
    {
        ValidationErrors = new List<string> { message };
    }

    public ValidationException(List<string> errors) : base("Validation failed")
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Database Operation Exception
/// </summary>
public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message) { }
    public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
}
