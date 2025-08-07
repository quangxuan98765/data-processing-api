namespace DataProcessingAPI.Domain.Entities;

/// <summary>
/// Revenue Entity - Domain model cho thu
/// Mapping tới table TaiChinh_ThuHoatDong
/// </summary>
public class RevenueEntity
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string LoaiThu { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}

/// <summary>
/// Expense Entity - Domain model cho chi
/// Mapping tới table TaiChinh_ChiHoatDong
/// </summary>
public class ExpenseEntity
{
    public int Id { get; set; }
    public int ThangTaiChinh { get; set; }
    public int NamTaiChinh { get; set; }
    public int IdNguon { get; set; }
    public string LoaiChi { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
