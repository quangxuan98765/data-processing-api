using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Shared.Constants;
using DataAccess;
using System.Data;
using System.Globalization;

namespace DataProcessingAPI.Application.Services.Financial;

/// <summary>
/// Expense Service - XỬ LÝ CHI
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IDatabaseService _database;

    // Mapping tên nguồn sang ID cho Expense
    private readonly Dictionary<string, int> _expenseNguonMap = new()
    {
        { "Chi lương, thu nhập", 5 },
        { "Chi cơ sở vật chất và dịch vụ", 6 },
        { "Chi hỗ trợ người học", 7 },
        { "Chi khác", 8 }
    };

    public ExpenseService(IDatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
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
            // Create DataTable và bulk insert
            var dataTable = CreateDataTable(data);
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
        return result.AsEnumerable().Select(MapDataRowToExpenseDto).ToList();
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
    private DataTable CreateDataTable(List<ExpenseImportDto> data)
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
                GetNguonId(item.TenNguon),
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

    private int GetNguonId(string tenNguon)
    {
        return _expenseNguonMap.TryGetValue(tenNguon ?? "", out int id) ? id : 0;
    }

    private ExpenseDto MapDataRowToExpenseDto(DataRow row)
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
            TenNguon = _expenseNguonMap.FirstOrDefault(x => x.Value == Convert.ToInt32(row["IdNguon"])).Key ?? ""
        };
    }
}
