namespace ExcelDataAPI.Application.DTOs;

/// <summary>
/// Revenue Import DTO - Clean input data
/// Thay thế Models.Revenue.RevenueDataRow
/// </summary>
public class RevenueImportDto
{
    public string ThangTaiChinh { get; set; } = string.Empty;
    public string NamTaiChinh { get; set; } = string.Empty;
    public string IdNguon { get; set; } = string.Empty;
    public string LoaiThu { get; set; } = string.Empty;
    public string SoTien { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string ThoiGianNhap { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Import DTO - Clean input data
/// Thay thế Models.Expense.ExpenseDataRow
/// </summary>
public class ExpenseImportDto
{
    public string ThangTaiChinh { get; set; } = string.Empty;
    public string NamTaiChinh { get; set; } = string.Empty;
    public string IdNguon { get; set; } = string.Empty;
    public string LoaiChi { get; set; } = string.Empty;
    public string SoTien { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Generic Import Request - Container cho bulk data
/// </summary>
public class FinancialImportRequestDto<T>
{
    public List<T> Data { get; set; } = new();
    public string IdNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
