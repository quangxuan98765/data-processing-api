using Microsoft.Data.SqlClient;
using System.Data;

namespace DataAccess;

/// <summary>
/// 🎯 DATABASE SERVICE - Xử lý tất cả database operations
/// Implement IDatabaseService interface để dễ test và maintain
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // ✅ QUERY NHIỀU RECORDS  
    public async Task<DataTable> QueryAsync(string sql, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        AddParameters(command, parameters);
        
        var dataTable = new DataTable();
        await connection.OpenAsync();
        
        using var reader = await command.ExecuteReaderAsync();
        dataTable.Load(reader);
        
        return dataTable;
    }

    // ✅ QUERY MỘT GIÁ TRỊ
    public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        AddParameters(command, parameters);
        
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        
        return result is T value ? value : default(T);
    }

    // ✅ EXECUTE (INSERT/UPDATE/DELETE)
    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        
        AddParameters(command, parameters);
        
        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    // ✅ EXECUTE STORED PROCEDURE
    public async Task<DataTable> ExecuteStoredProcAsync(string spName, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(spName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        
        AddParameters(command, parameters);
        
        var dataTable = new DataTable();
        await connection.OpenAsync();
        
        using var reader = await command.ExecuteReaderAsync();
        dataTable.Load(reader);
        
        return dataTable;
    }

    // ✅ BULK INSERT
    public async Task<int> BulkInsertAsync(DataTable dataTable, string tableName)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = tableName;
        
        // Map columns
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }
        
        await bulkCopy.WriteToServerAsync(dataTable);
        return dataTable.Rows.Count;
    }

    // ✅ EXECUTE TRONG TRANSACTION
    public async Task<T> ExecuteInTransactionAsync<T>(Func<SqlConnection, SqlTransaction, Task<T>> action)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var transaction = connection.BeginTransaction();
        try
        {
            var result = await action(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // Helper: Thêm parameters
    private static void AddParameters(SqlCommand command, object? parameters)
    {
        if (parameters == null) return;

        // Nếu là Dictionary<string, object>
        if (parameters is Dictionary<string, object> dict)
        {
            foreach (var param in dict)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
            return;
        }

        // Nếu là object properties
        foreach (var prop in parameters.GetType().GetProperties())
        {
            var value = prop.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.AddWithValue($"@{prop.Name}", value);
        }
    }
    
}
