using ExcelDataAPI.Models.Revenue;
using ExcelDataAPI.Models.Expense;

namespace ExcelDataAPI.Models.Common;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public RevenueDataRow? ValidatedRevenueRow { get; set; }
    public Models.Expense.ExpenseDataRow? ValidatedExpenseRow { get; set; }
    public ExcelOutputRow? OutputRow { get; set; }
}
