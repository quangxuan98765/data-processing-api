namespace DataProcessingAPI.Shared.Models;

public class ApiResponse<T>
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Operation completed successfully", int status = 200)
    {
        return new ApiResponse<T>
        {
            Status = status,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(string message, int status = 500, T? data = default)
    {
        return new ApiResponse<T>
        {
            Status = status,
            Message = message,
            Data = data
        };
    }
}

public class ApiResponse : ApiResponse<object?>
{
    public static ApiResponse Success(string message = "Operation completed successfully", int status = 200)
    {
        return new ApiResponse
        {
            Status = status,
            Message = message,
            Data = null
        };
    }

    public static new ApiResponse Error(string message, int status = 500)
    {
        return new ApiResponse
        {
            Status = status,
            Message = message,
            Data = null
        };
    }
}
