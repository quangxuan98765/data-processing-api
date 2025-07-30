namespace ExcelDataAPI.Models.Expense;

public class ExpenseInputRow
{
    public string? ThangTaiChinh { get; set; }
    public string? NamTaiChinh { get; set; }
    public string? TenNguon { get; set; }  // Mapping to IdNguon (5,6,7,8)
    public string? LoaiChi { get; set; }   // "Chi hoạt động"
    public string? SoTien { get; set; }
    public string? MoTa { get; set; }
    public string? GhiChu { get; set; }
}
