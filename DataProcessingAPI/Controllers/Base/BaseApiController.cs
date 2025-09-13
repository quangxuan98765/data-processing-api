using Microsoft.AspNetCore.Mvc;
using DataProcessingAPI.Shared.Models;

namespace DataProcessingAPI.Controllers.Base;

public abstract class BaseApiController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseApiController(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected IActionResult HandleSuccess<T>(T data, string message = "Operation completed successfully", int status = 200)
    {
        return StatusCode(status, new
        {
            status = status,
            message = message,
            data = data
        });
    }

    protected IActionResult HandleError(string message, int status = 500)
    {
        return StatusCode(status, new
        {
            status = status,
            message = message,
            data = (object?)null
        });
    }

    protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, string? successMessage = null, int successStatus = 200)
    {
        try
        {
            _logger.LogInformation("Starting {OperationName}", operationName);
            var result = await operation();
            _logger.LogInformation("Completed {OperationName} successfully", operationName);
            return HandleSuccess(result, successMessage ?? "Operation completed successfully", successStatus);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found in {OperationName}: {Message}", operationName, ex.Message);
            return HandleError(ex.Message, 404);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access in {OperationName}: {Message}", operationName, ex.Message);
            return HandleError(ex.Message, 401);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Bad request in {OperationName}: {Message}", operationName, ex.Message);
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return HandleError("An error occurred while processing your request");
        }
    }

    protected async Task<IActionResult> ExecuteAsync(Func<Task<int>> operation, string operationName, string successMessage, int successStatus = 200)
    {
        try
        {
            _logger.LogInformation("Starting {OperationName}", operationName);
            var result = await operation();
            
            if (result > 0)
            {
                _logger.LogInformation("Completed {OperationName} successfully", operationName);
                return HandleSuccess(new { id = result }, successMessage, successStatus);
            }

            _logger.LogWarning("{OperationName} failed - no rows affected", operationName);
            return HandleError($"Failed to {operationName.ToLower()}", 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return HandleError("An error occurred while processing your request");
        }
    }

    protected IActionResult ValidateModel()
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state");
            return BadRequest(new
            {
                status = 400,
                message = "Invalid input",
                data = ModelState
            });
        }
        return null!;
    }

    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    protected string GetAuthToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            throw new UnauthorizedAccessException("Missing or invalid authorization header");
        }
        return authHeader.Substring("Bearer ".Length).Trim();
    }
}
