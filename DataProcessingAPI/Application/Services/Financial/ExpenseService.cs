using System.Data;                                    // DataTable, DataRow
using DataProcessingAPI.Application.DTOs;            // ExpenseDto, ExpenseImportDto, BulkOperationResultDto
using DataProcessingAPI.Application.Interfaces.Financial;  // IExpenseService
using DataProcessingAPI.Shared.Constants;            // DatabaseConstants, AppConstants
using DataAccess;                                     // IDatabaseService

namespace DataProcessingAPI.Application.Services.Financial;

/// <summary>
/// Expense Service - XỬ LÝ CHI
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IDatabaseService _database;
    private Dictionary<string, int>? nguonMap; // 🆕 Load từ DB

    public ExpenseService(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    // 🆕 Load nguồn CHI từ DB (Loai = 2)
    private async Task<Dictionary<string, int>> GetNguonMapAsync()
    {
        if (nguonMap == null)
        {
            var result = await _database.ExecuteStoredProcAsync("sp_Get_NguonTaiChinh");

            nguonMap = result.AsEnumerable()
                .Where(row => Convert.ToInt32(row["Loai"]) == 2) // CHỈ lấy CHI (Loai = 2)
                .ToDictionary(
                    row => row["Ten"].ToString()!,
                    row => Convert.ToInt32(row["Id"])
                );
        }

        return nguonMap;
    }

    /// <summary>📊 BULK IMPORT EXPENSE FROM EXCEL</summary>
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
            var nguonMap = await GetNguonMapAsync();
            // Create DataTable và bulk insert
            var dataTable = CreateDataTable(data, nguonMap);
            var insertedRows = await _database.BulkInsertAsync(dataTable, DatabaseConstants.EXPENSE_TABLE);

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

    /// <summary>📋 GET ALL EXPENSES</summary>
    public async Task<List<ExpenseDto>> GetAllAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ThangTaiChinh", 0 },
            { "@NamTaiChinh", 0 },
            { "@IdNguon", 0 },
            { "@LoaiNguon", DatabaseConstants.LOAI_CHI }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_GET_THUCHIITAICHINH, parameters);
        var nguonMap = await GetNguonMapAsync();
        return result.AsEnumerable().Select(row => MapDataRowToExpenseDto(row, nguonMap)).ToList();
    }

    /// <summary>🔍 GET EXPENSE BY ID</summary>
    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var expenses = await GetAllAsync();
        return expenses.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>➕ CREATE EXPENSE</summary>
    public async Task<int> CreateAsync(ExpenseDto expense)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ThangTaiChinh", expense.ThangTaiChinh },
            { "@NamTaiChinh", expense.NamTaiChinh },
            { "@IdNguon", expense.IdNguon },
            { "@SoTien", expense.SoTien },
            { "@MoTa", expense.MoTa ?? "" },
            { "@GhiChu", expense.GhiChu ?? "" },
            { "@IDNguoiDung", expense.IDNguoiDung ?? "" },
            { "@NguoiNhap", expense.NguoiNhap ?? "" }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_INSERT_THUCHIHOATDONG, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    /// <summary>✏️ UPDATE EXPENSE</summary>
    public async Task<int> UpdateAsync(int id, ExpenseDto expense)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@Id", id },
            { "@ThangTaiChinh", expense.ThangTaiChinh },
            { "@NamTaiChinh", expense.NamTaiChinh },
            { "@IdNguon", expense.IdNguon },
            { "@SoTien", expense.SoTien },
            { "@MoTa", expense.MoTa ?? "" },
            { "@GhiChu", expense.GhiChu ?? "" },
            { "@IDNguoiDung", expense.IDNguoiDung ?? "" },
            { "@NguoiNhap", expense.NguoiNhap ?? "" }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_UPDATE_THUCHIHOATDONG, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    /// <summary>❌ DELETE EXPENSE</summary>
    public async Task<int> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@ID", id },
            { "@LoaiHoatDong", DatabaseConstants.LOAI_CHI }
        };

        var result = await _database.ExecuteStoredProcAsync(DatabaseConstants.SP_DELETE_THUCHIITAICHINH, parameters);
        return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["ReturnCode"]) : 0;
    }

    // Helper methods
    private DataTable CreateDataTable(List<ExpenseImportDto> data, Dictionary<string, int> nguonMap)
    {
        var table = new DataTable();
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

        foreach (var item in data)
        {
            table.Rows.Add(
                int.TryParse(item.ThangTaiChinh, out int month) ? month : 0,
                int.TryParse(item.NamTaiChinh, out int year) ? year : 0,
                GetNguonId(item.TenNguon, nguonMap),
                item.LoaiChi ?? "",
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

    private int GetNguonId(string tenNguon, Dictionary<string, int> nguonMap)
    {
        return nguonMap.TryGetValue(tenNguon ?? "", out int id) ? id : 0;
    }

    private ExpenseDto MapDataRowToExpenseDto(DataRow row, Dictionary<string, int> nguonMap)
    {
        return new ExpenseDto
        {
            Id = Convert.ToInt32(row["ID"]),
            ThangTaiChinh = Convert.ToInt32(row["ThangTaiChinh"]),
            NamTaiChinh = Convert.ToInt32(row["NamTaiChinh"]),
            IdNguon = Convert.ToInt32(row["IdNguon"]),
            LoaiChi = row["LoaiThuChi"]?.ToString() ?? "",
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
