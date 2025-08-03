using Microsoft.Data.SqlClient;
using System.Data;
using ExcelDataAPI.Infrastructure.Configuration;
using ExcelDataAPI.Shared.Exceptions;

namespace ExcelDataAPI.Infrastructure.Data;

/// <summary>
/// SQL Server Data Provider Implementation
/// Modernized từ legacy DataProvider với async/await
/// Chỉ implement 5 core methods cần thiết
/// </summary>
public class SqlDataProvider : IDataProvider
{
    private readonly DatabaseSettings _settings;

    public SqlDataProvider(DatabaseSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    #region Core Methods Implementation

    public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120)
    {
        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = timeoutSeconds
            };
            
            // ✅ CLEAN: Reusable parameter helper
            AddParameters(command, parameters);
            
            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Execute non-query failed: {ex.Message}", ex);
        }
    }

    public async Task<DataTable> GetDataTableAsync(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120)
    {
        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = timeoutSeconds
            };
            
            // ✅ CLEAN: Reusable parameter helper
            AddParameters(command, parameters);

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            
            return dataTable;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Get DataTable failed: {ex.Message}", ex);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object>? parameters = null, int timeoutSeconds = 120)
    {
        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(sql, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = timeoutSeconds
            };
            
            // ✅ CLEAN: Reusable parameter helper
            AddParameters(command, parameters);
            
            var result = await command.ExecuteScalarAsync();
            
            if (result == null || result == DBNull.Value)
                return default(T);
                
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Execute scalar failed: {ex.Message}", ex);
        }
    }

    public async Task<DataTable> ExecuteStoredProcedureAsync(string procedureName, 
        Dictionary<string, object>? parameters = null, int timeoutSeconds = 120)
    {
        try
        {
            using var connection = await GetConnectionAsync();
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = timeoutSeconds
            };
            
            // ✅ CLEAN: Reusable parameter helper
            AddParameters(command, parameters);
            
            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            
            return dataTable;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Execute stored procedure failed: {ex.Message}", ex);
        }
    }

    public async Task<bool> CheckDataExistsAsync(string sql, int timeoutSeconds = 30)
    {
        try
        {
            var result = await ExecuteScalarAsync<int>(sql, null, timeoutSeconds);
            return result > 0;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Check data exists failed: {ex.Message}", ex);
        }
    }

    public async Task<int> BulkInsertAsync(DataTable dataTable, string tableName, int batchSize = 1000, int timeoutSeconds = 300)
    {
        try
        {
            if (dataTable.Rows.Count == 0)
                return 0;

            using var connection = await GetConnectionAsync();
            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = tableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeoutSeconds
            };

            // Auto-map columns based on DataTable column names
            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(dataTable);
            return dataTable.Rows.Count;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Bulk insert failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<SqlConnection> GetConnectionAsync()
    {
        try
        {
            var connection = new SqlConnection(_settings.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Connection failed: {ex.Message}", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// ✅ CLEAN & REUSABLE: Add parameters to SqlCommand
    /// Prevents code duplication and ensures consistent parameter handling
    /// </summary>
    private static void AddParameters(SqlCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null) return;

        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        }
    }

    #endregion
}
