using ExcelDataAPI.Models.Common;

namespace ExcelDataAPI.Models.Common;

public class ImportResponse
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
