namespace ExcelDataAPI.Models;

public class BulkInsertRequest
{
    public List<ExcelInputRow> Data { get; set; } = new();
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
