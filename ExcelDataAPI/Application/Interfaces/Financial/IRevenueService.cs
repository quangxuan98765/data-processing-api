using ExcelDataAPI.Application.DTOs;

namespace ExcelDataAPI.Application.Interfaces.Financial;

/// <summary>
/// Revenue Service Interface - Chỉ xử lý logic Revenue
/// Tách biệt hoàn toàn với Expense
/// </summary>
public interface IRevenueService
{
    /// <summary>
    /// Bulk insert revenue data with built-in validation
    /// </summary>
    Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data);
}
