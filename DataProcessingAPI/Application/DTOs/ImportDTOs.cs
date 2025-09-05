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
/// Revenue Entity DTO - For Service Layer (INTERNAL)
/// Giữ để tương thích với Service layer hiện tại
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
/// Revenue Request DTO - For CREATE and UPDATE operations
/// Covers both POST /api/revenue and PUT /api/revenue/{id}
/// </summary>
public class RevenueRequest
{
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string LoaiThu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Entity DTO - For Service Layer (INTERNAL)
/// Giữ để tương thích với Service layer hiện tại
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
/// Revenue Response DTO - For API responses (GET operations)
/// </summary>
public class RevenueResponse
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
/// Expense Request DTO - For CREATE and UPDATE operations
/// </summary>
public class ExpenseRequest
{
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string LoaiChi { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Response DTO - For API responses (GET operations)
/// </summary>
public class ExpenseResponse
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
