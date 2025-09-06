using System.Data;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Application.Interfaces.Financial;
using DataAccess;            

namespace DataProcessingAPI.Application.Services.Financial;

/// <summary>
/// Revenue Service - XỬ LÝ THU - GỌN NHƯ EXPENSE
/// </summary>
public class RevenueService : IRevenueService
{
    private readonly IDatabaseService _database;
    private Dictionary<string, int>? nguonMap; // 🆕 Sẽ load từ DB

    public RevenueService(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>🔄 LOAD NGUỒN THU TỪ DB (CHỈ GỌI 1 LẦN)</summary>
    private async Task<Dictionary<string, int>> GetNguonMapAsync()
    {
        if (nguonMap == null)
        {
            var result = await _database.ExecuteStoredProcAsync("sp_Get_NguonTaiChinh");

            nguonMap = result.AsEnumerable()
                .Where(row => Convert.ToInt32(row["Loai"]) == 1) // Chỉ lấy THU (Loai = 1)
                .ToDictionary(
                    row => row["Ten"].ToString()!,
                    row => Convert.ToInt32(row["Id"])
                );
        }

        return nguonMap;
    }

    /// <summary>📊 BULK IMPORT REVENUE FROM EXCEL</summary>
    public async Task<BulkOperationResultDto> BulkInsertAsync(List<RevenueImportDto> data)
    {
        var result = new BulkOperationResultDto { TotalRows = data.Count };

        if (!data.Any())
        {
            result.AddError("Không có dữ liệu để xử lý.");
            return result;
        }

        try
        {
            // 🆕 Load nguồn từ DB
            var nguonMapData = await GetNguonMapAsync();
            var dataTable = CreateDataTable(data, nguonMapData);

            var insertedRows = await _database.BulkInsertAsync(dataTable, "ThuChiTaiChinh");

            result.Success = true;
            result.Message = "Thêm dữ liệu thu khối lượng lớn thành công.";
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
            { "@LoaiNguon", "THU" }
        };

        var result = await _database.ExecuteStoredProcAsync("sp_Get_ThuChiTaiChinh", parameters);
        var nguonMapData = await GetNguonMapAsync(); // 🆕 Load nguồn để map tên

        return result.AsEnumerable().Select(row => MapDataRowToRevenueDto(row, nguonMapData)).ToList();
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

        var result = await _database.ExecuteStoredProcAsync("sp_Insert_ThuChiTaiChinh", parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["NewId"]) : 0;
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

        var result = await _database.ExecuteStoredProcAsync("sp_Update_ThuChiTaiChinh", parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["NewId"]) : 0;
    }

    /// <summary>❌ DELETE REVENUE</summary>
    public async Task<int> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ID", id },
            { "@LoaiHoatDong", "THU" }
        };

        var result = await _database.ExecuteStoredProcAsync("sp_Delete_ThuChiTaiChinh", parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["NewId"]) : 0;
    }

    // Helper methods
    private DataTable CreateDataTable(List<RevenueImportDto> data, Dictionary<string, int> nguonMap)
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
                GetNguonId(item.TenNguon, nguonMap),
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

    private static int GetNguonId(string tenNguon, Dictionary<string, int> nguonMap)
    {
        return nguonMap.TryGetValue(tenNguon ?? "", out int id) ? id : 0;
    }

    private static RevenueDto MapDataRowToRevenueDto(DataRow row, Dictionary<string, int> nguonMap)
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
            TenNguon = nguonMap.FirstOrDefault(x => x.Value == Convert.ToInt32(row["IdNguon"])).Key ?? ""
        };
    }
}