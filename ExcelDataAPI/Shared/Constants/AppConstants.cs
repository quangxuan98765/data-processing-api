namespace ExcelDataAPI.Shared.Constants;

/// <summary>
/// Database Constants - Thay thế hardcoded strings
/// </summary>
public static class DatabaseConstants
{
    // Table Names
    public const string REVENUE_TABLE = "dbo.TaiChinh_ThuHoatDong";
    public const string EXPENSE_TABLE = "dbo.TaiChinh_ChiHoatDong";

    // Default Timeouts
    public const int DEFAULT_COMMAND_TIMEOUT = 120;
    public const int DEFAULT_BULK_TIMEOUT = 300;
    public const int DEFAULT_VALIDATION_TIMEOUT = 30;

    // Batch Sizes
    public const int DEFAULT_BATCH_SIZE = 1000;
    public const int MAX_BATCH_SIZE = 10000;
}

/// <summary>
/// Application Constants
/// </summary>
public static class AppConstants
{
    // Error Messages
    public const string NO_DATA_ERROR = "Không có dữ liệu để xử lý";
    public const string VALIDATION_ERROR = "Dữ liệu không hợp lệ";
    public const string DATABASE_ERROR = "Lỗi database";

    // Success Messages  
    public const string BULK_INSERT_SUCCESS = "Bulk insert thành công";
    public const string VALIDATION_SUCCESS = "Validation thành công";
}
