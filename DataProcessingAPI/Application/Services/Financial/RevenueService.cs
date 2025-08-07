using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Shared.Constants;
using DataAccess;
using System.Data;

namespace DataProcessingAPI.Application.Services.Financial;

/// <summary>
/// Revenue Service - XỬ LÝ THU - GỌN NHƯ EXPENSE
/// </summary>
public class RevenueService : IRevenueService
{
    private readonly IDatabaseService _database;

    // Mapping tên nguồn sang ID cho Revenue
    private readonly Dictionary<string, int> _nguonMap = new()
    {
        { "Hỗ trợ chi thường xuyên từ Nhà nước/nhà đầu tư", 1 },
        { "Thu giáo dục và đào tạo", 2 },
        { "Thu khoa học và công nghệ", 3 },
        { "Thu nhập khác (thu nhập ròng)", 4 }
    };

    public RevenueService(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>📊 BULK IMPORT REVENUE FROM EXCEL</summary>
    public async Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data)
    {
        var result = new BulkOperationResultDto { TotalRows = data.Count };
        
        if (!data.Any())
        {
            result.AddError(AppConstants.NO_DATA_ERROR);
            return result;
        }

        try
        {
            var dataTable = CreateDataTable(data);
            var insertedRows = await _database.BulkInsertAsync(dataTable, DatabaseConstants.REVENUE_TABLE);
            
            result.Success = true;
            result.Message = $"Revenue {AppConstants.BULK_INSERT_SUCCESS}";
            result.ProcessedRows = data.Count;
            result.InsertedRows = insertedRows;
        }
        catch (Exception ex)
        {
            result.AddError($"Revenue bulk insert failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>📋 GET ALL REVENUES</summary>
    public async Task<List<RevenueDto>> GetAllAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ThangTaiChinh", 0 },
            { "@NamTaiChinh", 0 },
            { "@IdNguon", 0 },
            { "@LoaiNguon", DatabaseConstants.LOAI_THU }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_GET_THUCHIITAICHINH, parameters);
        return result.AsEnumerable().Select(MapDataRowToRevenueDto).ToList();
    }

    /// <summary>🔍 GET REVENUE BY ID</summary>
    public async Task<RevenueDto?> GetByIdAsync(int id)
    {
        var revenues = await GetAllAsync();
        return revenues.FirstOrDefault(r => r.Id == id);
    }

    /// <summary>➕ CREATE REVENUE</summary>
    public async Task<int> CreateAsync(RevenueDto revenue)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ThangTaiChinh", revenue.ThangTaiChinh },
            { "@NamTaiChinh", revenue.NamTaiChinh },
            { "@IdNguon", revenue.IdNguon },
            { "@SoTien", revenue.SoTien },
            { "@MoTa", revenue.MoTa ?? "" },
            { "@GhiChu", revenue.GhiChu ?? "" },
            { "@IDNguoiDung", revenue.IDNguoiDung ?? "" },
            { "@NguoiNhap", revenue.NguoiNhap ?? "" }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_INSERT_THUCHIHOATDONG, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    /// <summary>✏️ UPDATE REVENUE</summary>
    public async Task<int> UpdateAsync(int id, RevenueDto revenue)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@Id", id },
            { "@ThangTaiChinh", revenue.ThangTaiChinh },
            { "@NamTaiChinh", revenue.NamTaiChinh },
            { "@IdNguon", revenue.IdNguon },
            { "@SoTien", revenue.SoTien },
            { "@MoTa", revenue.MoTa ?? "" },
            { "@GhiChu", revenue.GhiChu ?? "" },
            { "@IDNguoiDung", revenue.IDNguoiDung ?? "" },
            { "@NguoiNhap", revenue.NguoiNhap ?? "" }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_UPDATE_THUCHIHOATDONG, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    /// <summary>❌ DELETE REVENUE</summary>
    public async Task<int> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ID", id },
            { "@LoaiHoatDong", DatabaseConstants.LOAI_THU }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_DELETE_THUCHIITAICHINH, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    // Helper methods
    private DataTable CreateDataTable(List<RevenueImportDto> data)
    {
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

        foreach (var item in data)
        {
            table.Rows.Add(
                int.TryParse(item.ThangTaiChinh, out int month) ? month : 0,
                int.TryParse(item.NamTaiChinh, out int year) ? year : 0,
                GetNguonId(item.TenNguon),
                item.LoaiThu ?? "",
                decimal.TryParse(item.SoTien, out decimal amount) ? amount : 0m,
                item.MoTa ?? "",
                item.GhiChu ?? "",
                DateTime.Now,
                item.IDNguoiDung ?? "",
                item.NguoiNhap ?? ""
            );
        }

        return table;
    }

    private int GetNguonId(string tenNguon)
    {
        return _nguonMap.TryGetValue(tenNguon ?? "", out int id) ? id : 0;
    }

    private RevenueDto MapDataRowToRevenueDto(DataRow row)
    {
        return new RevenueDto
        {
            Id = Convert.ToInt32(row["ID"]),
            ThangTaiChinh = Convert.ToInt32(row["ThangTaiChinh"]),
            NamTaiChinh = Convert.ToInt32(row["NamTaiChinh"]),
            IdNguon = Convert.ToInt32(row["IdNguon"]),
            LoaiThu = row["LoaiThuChi"]?.ToString() ?? "",
            SoTien = Convert.ToDecimal(row["SoTien"]),
            MoTa = row["MoTa"]?.ToString() ?? "",
            GhiChu = row["GhiChu"]?.ToString() ?? "",
            ThoiGianNhap = Convert.ToDateTime(row["ThoiGianNhap"]),
            IDNguoiDung = row["IDNguoiDung"]?.ToString() ?? "",
            NguoiNhap = row["NguoiNhap"]?.ToString() ?? "",
            TenNguon = _nguonMap.FirstOrDefault(x => x.Value == Convert.ToInt32(row["IdNguon"])).Key ?? ""
        };
    }
}
