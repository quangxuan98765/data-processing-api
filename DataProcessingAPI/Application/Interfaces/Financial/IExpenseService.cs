using DataProcessingAPI.Application.DTOs;

namespace DataProcessingAPI.Application.Interfaces.Financial;

/// <summary>
/// Expense Service Interface - EXPENSE IMPORT + CRUD ĐƠN GIẢN
/// </summary>
public interface IExpenseService
{
    /// <summary>Bulk insert expense data with validation</summary>
    Task<BulkOperationResultDto> BulkInsertAsync(List<ExpenseImportDto> data);
    
    // ✅ CRUD Operations
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<List<ExpenseDto>> GetAllAsync();
    Task<int> CreateAsync(ExpenseDto expense);
    Task<int> UpdateAsync(int id, ExpenseDto expense);
    Task<int> DeleteAsync(int id);
}
