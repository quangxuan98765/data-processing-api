using ExcelDataAPI.Application.Interfaces.Financial;
using ExcelDataAPI.Application.DTOs;
using ExcelDataAPI.Infrastructure.Data;
using ExcelDataAPI.Shared.Constants;
using ExcelDataAPI.Shared.Exceptions;
using System.Data;
using System.Globalization;

namespace ExcelDataAPI.Application.Services.Financial;

/// <summary>
/// Expense Service Implementation - CHỈ XỬ LÝ EXPENSE
/// Tách biệt hoàn toàn với Revenue để dễ maintain và debug
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IDataProvider _dataProvider;

    public ExpenseService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    #region Public Methods

    public async Task<BulkOperationResultDto> BulkInsertAsync(List<ExpenseImportDto> data)
    {
        var result = new BulkOperationResultDto { TotalRows = data.Count };
        
        if (!data.Any())
        {
            result.AddError(AppConstants.NO_DATA_ERROR);
            return result;
        }

        try
        {
            // Step 1: Validate data (inline validation)
            var validationResult = new ValidationResultDto();
            
            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                int rowNumber = i + 1;
                
                // Required fields validation
                ValidateRequiredFields(row, rowNumber, validationResult);
                
                // Data type validation  
                ValidateDataTypes(row, rowNumber, validationResult);
            }
            
            if (!validationResult.IsValid)
            {
                result.Success = false;
                result.Message = AppConstants.VALIDATION_ERROR;
                result.Errors.AddRange(validationResult.Errors);
                return result;
            }

            // Step 2: Create DataTable
            var dataTable = CreateDataTable(data);
            
            // Step 3: Bulk insert
            var insertedRows = await _dataProvider.BulkInsertAsync(dataTable, DatabaseConstants.EXPENSE_TABLE);
            
            // Step 4: Return success result
            result.Success = true;
            result.Message = $"Expense {AppConstants.BULK_INSERT_SUCCESS}";
            result.ProcessedRows = data.Count;
            result.InsertedRows = insertedRows;
        }
        catch (Exception ex)
        {
            result.AddError($"Expense bulk insert failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> CheckDataExistsAsync(int month, int year, int sourceId)
    {
        try
        {
            // ✅ SECURE: Using parameterized query to prevent SQL injection
            var sql = @"SELECT COUNT(1) FROM dbo.TaiChinh_ChiHoatDong 
                       WHERE ThangTaiChinh = @month 
                       AND NamTaiChinh = @year 
                       AND IdNguon = @sourceId";
            
            var parameters = CreatePeriodParameters(month, year, sourceId);
            var count = await _dataProvider.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }
        catch (Exception)
        {
            return false; // Assume not exists if check fails
        }
    }

    public async Task<List<ExpenseImportDto>> GetByPeriodAsync(int month, int year)
    {
        try
        {
            // ✅ SECURE: Using parameterized query to prevent SQL injection
            var sql = @"SELECT * FROM dbo.TaiChinh_ChiHoatDong 
                       WHERE ThangTaiChinh = @month AND NamTaiChinh = @year";
            
            var parameters = CreatePeriodParameters(month, year);
            var dataTable = await _dataProvider.GetDataTableAsync(sql, parameters);
            return MapDataTableToDto(dataTable);
        }
        catch (Exception)
        {
            return new List<ExpenseImportDto>();
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// ✅ CLEAN & REUSABLE: Create period parameters dictionary
    /// Prevents code duplication and ensures consistent parameter naming
    /// </summary>
    private static Dictionary<string, object> CreatePeriodParameters(int month, int year, int? sourceId = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["@month"] = month,
            ["@year"] = year
        };
        
        if (sourceId.HasValue)
        {
            parameters["@sourceId"] = sourceId.Value;
        }
        
        return parameters;
    }

    #endregion

    #region Private Validation Methods

    private static void ValidateRequiredFields(ExpenseImportDto item, int rowNumber, ValidationResultDto result)
    {
        if (string.IsNullOrWhiteSpace(item.ThangTaiChinh))
            result.AddError($"Dòng {rowNumber}: Tháng tài chính không được trống");
            
        if (string.IsNullOrWhiteSpace(item.NamTaiChinh))
            result.AddError($"Dòng {rowNumber}: Năm tài chính không được trống");
            
        if (string.IsNullOrWhiteSpace(item.SoTien))
            result.AddError($"Dòng {rowNumber}: Số tiền không được trống");

        if (string.IsNullOrWhiteSpace(item.LoaiChi))
            result.AddError($"Dòng {rowNumber}: Loại chi không được trống");
    }

    private static void ValidateDataTypes(ExpenseImportDto item, int rowNumber, ValidationResultDto result)
    {
        // Validate month
        if (!int.TryParse(item.ThangTaiChinh, out int month) || month < 1 || month > 12)
            result.AddError($"Dòng {rowNumber}: Tháng phải từ 1-12");
            
        // Validate year
        if (!int.TryParse(item.NamTaiChinh, out int year) || year < 2000)
            result.AddError($"Dòng {rowNumber}: Năm phải >= 2000");
            
        // Validate amount
        if (!decimal.TryParse(item.SoTien, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount < 0)
            result.AddError($"Dòng {rowNumber}: Số tiền phải >= 0");

        // Validate source ID
        if (!string.IsNullOrWhiteSpace(item.IdNguon) && (!int.TryParse(item.IdNguon, out int sourceId) || sourceId <= 0))
            result.AddError($"Dòng {rowNumber}: ID nguồn phải là số nguyên dương");
    }

    private async Task CheckForDuplicates(ExpenseImportDto item, int rowNumber, ValidationResultDto result)
    {
        if (!string.IsNullOrWhiteSpace(item.IdNguon) && 
            int.TryParse(item.ThangTaiChinh, out int month) &&
            int.TryParse(item.NamTaiChinh, out int year) &&
            int.TryParse(item.IdNguon, out int sourceId))
        {
            var exists = await CheckDataExistsAsync(month, year, sourceId);
            if (exists)
                result.AddWarning($"Dòng {rowNumber}: Expense data có thể đã tồn tại (Tháng: {month}, Năm: {year}, Nguồn: {sourceId})");
        }
    }

    #endregion

    #region Private Helper Methods

    private static DataTable CreateDataTable(List<ExpenseImportDto> data)
    {
        var table = new DataTable();
        
        // Define columns (skip ID - auto increment)
        table.Columns.Add("ThangTaiChinh", typeof(int));
        table.Columns.Add("NamTaiChinh", typeof(int));
        table.Columns.Add("IdNguon", typeof(int));
        table.Columns.Add("LoaiChi", typeof(string));
        table.Columns.Add("SoTien", typeof(decimal));
        table.Columns.Add("MoTa", typeof(string));
        table.Columns.Add("GhiChu", typeof(string));
        table.Columns.Add("ThoiGianNhap", typeof(DateTime));
        table.Columns.Add("IDNguoiDung", typeof(string));
        table.Columns.Add("NguoiNhap", typeof(string));

        // Add data rows
        foreach (var item in data)
        {
            table.Rows.Add(
                ParseInt(item.ThangTaiChinh),
                ParseInt(item.NamTaiChinh),
                ParseInt(item.IdNguon),
                item.LoaiChi ?? string.Empty,
                ParseDecimal(item.SoTien),
                item.MoTa ?? string.Empty,
                item.GhiChu ?? string.Empty,
                DateTime.Now, // Always use current time for expenses
                item.IDNguoiDung ?? string.Empty,
                item.NguoiNhap ?? string.Empty
            );
        }

        return table;
    }

    private static List<ExpenseImportDto> MapDataTableToDto(DataTable dataTable)
    {
        var result = new List<ExpenseImportDto>();
        
        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(new ExpenseImportDto
            {
                ThangTaiChinh = row["ThangTaiChinh"].ToString() ?? string.Empty,
                NamTaiChinh = row["NamTaiChinh"].ToString() ?? string.Empty,
                IdNguon = row["IdNguon"].ToString() ?? string.Empty,
                LoaiChi = row["LoaiChi"].ToString() ?? string.Empty,
                SoTien = row["SoTien"].ToString() ?? string.Empty,
                MoTa = row["MoTa"].ToString() ?? string.Empty,
                GhiChu = row["GhiChu"].ToString() ?? string.Empty,
                IDNguoiDung = row["IDNguoiDung"].ToString() ?? string.Empty,
                NguoiNhap = row["NguoiNhap"].ToString() ?? string.Empty
            });
        }
        
        return result;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
    }

    private static DateTime ParseDateTime(string value)
    {
        return DateTime.TryParse(value, out DateTime result) ? result : DateTime.Now;
    }

    #endregion
}
