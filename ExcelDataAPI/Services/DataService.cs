using Microsoft.Data.SqlClient;
using System.Data;
using ExcelDataAPI.Models;

namespace ExcelDataAPI.Services;

public class DataService
{
    private readonly string _connectionString;

    public DataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<BulkInsertResult> BulkInsertToSqlServer(List<FinancialDataRow> validRows)
    {
        var result = new BulkInsertResult();
        
        if (!validRows.Any())
        {
            result.Success = false;
            result.Message = "Không có dữ liệu hợp lệ để insert";
            return result;
        }

        try
        {
            // Tạo DataTable
            var table = new DataTable();
            table.Columns.Add("ThangTaiChinh", typeof(int));
            table.Columns.Add("NamTaiChinh", typeof(int));
            table.Columns.Add("IdNguon", typeof(int));
            table.Columns.Add("LoaiThu", typeof(string));
            table.Columns.Add("SoTien", typeof(decimal));
            table.Columns.Add("MoTa", typeof(string));
            table.Columns.Add("GhiChu", typeof(string));
            table.Columns.Add("ThoiGianNhap", typeof(DateTime));
            table.Columns.Add("IDNguoiDung", typeof(string));
            table.Columns.Add("NguoiNhap", typeof(string));

            // Thêm dữ liệu vào DataTable
            foreach (var row in validRows)
            {
                try
                {
                    table.Rows.Add(
                        ParseInt(row.ThangTaiChinh),
                        ParseInt(row.NamTaiChinh),
                        ParseInt(row.IdNguon),
                        row.LoaiThu,
                        ParseDecimal(row.SoTien),
                        row.MoTa,
                        row.GhiChu,
                        string.IsNullOrEmpty(row.ThoiGianNhap) ? DateTime.Now : ParseDateTime(row.ThoiGianNhap),
                        row.IDNguoiDung,
                        row.NguoiNhap
                    );
                    result.ProcessedRows++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing row: {ex.Message}");
                    result.FailedRows++;
                }
            }

            // Bulk insert vào SQL Server
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "dbo.TaiChinh_ThuHoatDong",
                BatchSize = 1000,
                BulkCopyTimeout = 300
            };

            // Explicit column mapping - BỎ column ID vì auto-increment
            bulkCopy.ColumnMappings.Add("ThangTaiChinh", "ThangTaiChinh");
            bulkCopy.ColumnMappings.Add("NamTaiChinh", "NamTaiChinh");
            bulkCopy.ColumnMappings.Add("IdNguon", "IdNguon");
            bulkCopy.ColumnMappings.Add("LoaiThu", "LoaiThu");
            bulkCopy.ColumnMappings.Add("SoTien", "SoTien");
            bulkCopy.ColumnMappings.Add("MoTa", "MoTa");
            bulkCopy.ColumnMappings.Add("GhiChu", "GhiChu");
            bulkCopy.ColumnMappings.Add("ThoiGianNhap", "ThoiGianNhap");
            bulkCopy.ColumnMappings.Add("IDNguoiDung", "IDNguoiDung");
            bulkCopy.ColumnMappings.Add("NguoiNhap", "NguoiNhap");

            await bulkCopy.WriteToServerAsync(table);

            result.Success = true;
            result.Message = "Bulk insert thành công";
            result.InsertedRows = result.ProcessedRows;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Bulk insert failed: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, out decimal result) ? result : 0m;
    }

    private static DateTime ParseDateTime(string value)
    {
        return DateTime.TryParse(value, out DateTime result) ? result : DateTime.Now;
    }
}

public class BulkInsertResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedRows { get; set; }
    public int InsertedRows { get; set; }
    public int FailedRows { get; set; }
    public List<string> Errors { get; set; } = new();
}
