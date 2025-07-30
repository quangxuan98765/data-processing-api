namespace ExcelDataAPI.Models.Revenue;

public class RevenueImportRequest
{
    public List<RevenueInputRow> Data { get; set; } = new();
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
