namespace ExcelDataAPI.Models.Expense;

public class ExpenseImportRequest
{
    public List<ExpenseInputRow> Data { get; set; } = new();
    public string IDNguoiDung { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
}
