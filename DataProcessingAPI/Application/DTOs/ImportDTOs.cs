namespace DataProcessingAPI.Application.DTOs;

/// <summary>
/// Revenue Import DTO - Clean input data
/// Thay thế Models.Revenue.RevenueDataRow
/// </summary>
public class RevenueImportDto
{
    public string ThangTaiChinh { get; set; } = string.Empty;
    public string NamTaiChinh { get; set; } = string.Empty;
    public string TenNguon { get; set; } = string.Empty;
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
    public string TenNguon { get; set; } = string.Empty;
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

/// <summary>
/// Revenue Entity DTO - For CRUD operations
/// Đại diện cho 1 record trong database
/// </summary>
public class RevenueDto
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string TenNguon { get; set; } = string.Empty; // For display
    public string LoaiThu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Revenue Create Request DTO
/// User nhập TenNguon, hệ thống convert sang IdNguon
/// </summary>
public class CreateRevenueDto
{
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public string TenNguon { get; set; } = string.Empty; // User input
    public string LoaiThu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Revenue Update Request DTO
/// </summary>
public class UpdateRevenueDto
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public string TenNguon { get; set; } = string.Empty; // User input
    public string LoaiThu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Operation Result DTO for single record operations
/// </summary>
public class OperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public object? Data { get; set; }
    
    public void AddError(string error)
    {
        Success = false;
        Errors.Add(error);
    }
}

/// <summary>
/// Expense Entity DTO - For CRUD operations
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string TenNguon { get; set; } = string.Empty;
    public string LoaiChi { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Create Request DTO
/// </summary>
public class CreateExpenseDto
{
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public string TenNguon { get; set; } = string.Empty;
    public string LoaiChi { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Update Request DTO
/// </summary>
public class UpdateExpenseDto
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public string TenNguon { get; set; } = string.Empty;
    public string LoaiChi { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
