namespace ExcelDataAPI.Models;

public class BulkInsertResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int InsertedRows { get; set; }
    public List<ValidationResult> ValidationDetails { get; set; } = new();
    public List<ExcelOutputRow> ExcelOutputData { get; set; } = new();
    public string? ErrorDetail { get; set; }
}
