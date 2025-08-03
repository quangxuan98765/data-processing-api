namespace ExcelDataAPI.Infrastructure.Configuration;

/// <summary>
/// Database Settings - Thay thế ConnectConfig legacy
/// Sử dụng với IConfiguration pattern
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    /// <summary>
    /// Connection string từ appsettings.json
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Default command timeout (seconds)
    /// </summary>
    public int CommandTimeout { get; set; } = 120;

    /// <summary>
    /// Batch size cho bulk operations
    /// </summary>
    public int BulkInsertBatchSize { get; set; } = 1000;

    /// <summary>
    /// Bulk copy timeout (seconds)
    /// </summary>
    public int BulkCopyTimeout { get; set; } = 300;

    /// <summary>
    /// Max retry attempts for failed operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Enable connection pooling
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;
}
