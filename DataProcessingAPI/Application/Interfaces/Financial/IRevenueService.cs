using DataProcessingAPI.Application.DTOs;

namespace DataProcessingAPI.Application.Interfaces.Financial;

/// <summary>
/// Revenue Service Interface - EXCEL IMPORT + CRUD ĐơN GIẢN
/// </summary>
public interface IRevenueService
{
    /// <summary>Bulk insert revenue data with validation</summary>
    Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data);
    
    // ✅ CRUD Operations
    Task<RevenueDto?> GetByIdAsync(int id);
    Task<List<RevenueDto>> GetAllAsync();
    Task<int> CreateAsync(RevenueDto revenue);
    Task<int> UpdateAsync(int id, RevenueDto revenue);
    Task<int> DeleteAsync(int id);
}
