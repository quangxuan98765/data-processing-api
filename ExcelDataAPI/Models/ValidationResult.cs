namespace ExcelDataAPI.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public FinancialDataRow? ValidatedRow { get; set; }
    public ExcelOutputRow? OutputRow { get; set; }
}
