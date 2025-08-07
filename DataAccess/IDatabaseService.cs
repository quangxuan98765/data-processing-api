using System.Data;
using Microsoft.Data.SqlClient;

namespace DataAccess;

/// <summary>
/// 🎯 DATABASE ACCESS INTERFACE - Dễ hiểu, dễ test
/// </summary>
public interface IDatabaseService
{
    // ✅ Query operations
    Task<DataTable> QueryAsync(string sql, object? parameters = null);
    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null);
    
    // ✅ Execute operations  
    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<DataTable> ExecuteStoredProcAsync(string spName, object? parameters = null);
    
    // ✅ Bulk operations
    Task<int> BulkInsertAsync(DataTable dataTable, string tableName);
    
    // ✅ Transaction support
    Task<T> ExecuteInTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> action);

}
