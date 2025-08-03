using ExcelDataAPI.Application.DTOs;

namespace ExcelDataAPI.Application.Interfaces.Financial;

/// <summary>
/// Expense Service Interface - Chỉ xử lý logic Expense
/// Tách biệt hoàn toàn với Revenue
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// Bulk insert expense data with built-in validation
    /// </summary>
    Task<BulkOperationResultDto> BulkInsertAsync(List<ExpenseImportDto> data);
}
