namespace ExcelDataAPI.Application.DTOs;

/// <summary>
/// Bulk Operation Result - Thay thế BulkInsertResult hiện tại
/// Standardized response cho mọi bulk operations
/// </summary>
public class BulkOperationResultDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int InsertedRows { get; set; }
    public int FailedRows { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Add error and mark as failed
    /// </summary>
    public void AddError(string error)
    {
        Success = false;
        Errors.Add(error);
        FailedRows++;
    }

    /// <summary>
    /// Add warning (không fail operation)
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

/// <summary>
/// Validation Result - Cho validation operations
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidatedData { get; set; } = new();

    /// <summary>
    /// Add validation error
    /// </summary>
    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }

    /// <summary>
    /// Add validation warning
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}
