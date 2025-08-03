using System.Data;

namespace ExcelDataAPI.Infrastructure.Data;

/// <summary>
/// Core Data Provider Interface - Thay thế DataProvider legacy
/// Chỉ 5 methods cần thiết cho project hiện tại
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Execute INSERT/UPDATE/DELETE commands with parameters
    /// ✅ SECURE: Parameterized queries prevent SQL injection
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120);

    /// <summary>
    /// Get DataTable từ SELECT query with parameters
    /// ✅ SECURE: Parameterized queries prevent SQL injection
    /// </summary>
    Task<DataTable> GetDataTableAsync(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120);

    /// <summary>
    /// Get single value with parameters - generic cho mọi data type
    /// ✅ SECURE: Parameterized queries prevent SQL injection
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120);

    /// <summary>
    /// Execute Stored Procedure với parameters
    /// Thay thế ReadDataAddPram từ legacy
    /// </summary>
    Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, 
        Dictionary<string, object>? parameters = null, int timeoutSeconds = 120);

    /// <summary>
    /// Check data exists - for validation
    /// Thay thế KiemTraDuLieuTrung từ legacy
    /// </summary>
    Task<bool> CheckDataExistsAsync(string sql, int timeoutSeconds = 30);

    /// <summary>
    /// Bulk insert DataTable - for performance
    /// Sử dụng từ DataService hiện tại
    /// </summary>
    Task<int> BulkInsertAsync(DataTable dataTable, string tableName, int batchSize = 1000, int timeoutSeconds = 300);
}
