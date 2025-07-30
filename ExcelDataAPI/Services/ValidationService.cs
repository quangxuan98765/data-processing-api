using ExcelDataAPI.Models.Common;
using ExcelDataAPI.Models.Revenue;
using ExcelDataAPI.Models.Expense;

namespace ExcelDataAPI.Services;

public class ValidationService
{
    private readonly Dictionary<string, int> _nguonMap = new()
    {
        { "Hỗ trợ chi thường xuyên từ Nhà nước/nhà đầu tư", 1 },
        { "Thu giáo dục và đào tạo", 2 },
        { "Thu khoa học và công nghệ", 3 },
        { "Thu nhập khác (thu nhập ròng)", 4 }
    };
    
    private readonly Dictionary<string, int> _expenseNguonMap = new()
    {
        { "Chi lương, thu nhập", 5 },
        { "Chi cơ sở vật chất và dịch vụ", 6 },
        { "Chi hỗ trợ người học", 7 },
        { "Chi khác", 8 }
    };

    private readonly List<string> _revenueRequiredColumns = new()
    {
        "ThangTaiChinh", "NamTaiChinh", "TenNguon", "LoaiThu", "SoTien", "MoTa", "GhiChu"
    };
    
    private readonly List<string> _expenseRequiredColumns = new()
    {
        "ThangTaiChinh", "NamTaiChinh", "TenNguon", "LoaiChi", "SoTien", "MoTa", "GhiChu"
    };

    public bool ValidateRevenueExcelStructure(List<RevenueInputRow> data)
    {
        if (!data.Any()) return false;

        var properties = typeof(RevenueInputRow).GetProperties().Select(p => p.Name).ToList();
        
        return _revenueRequiredColumns.All(col => properties.Contains(col));
    }
    
    public bool ValidateExpenseExcelStructure(List<ExpenseInputRow> data)
    {
        if (!data.Any()) return false;

        var properties = typeof(ExpenseInputRow).GetProperties().Select(p => p.Name).ToList();
        
        return _expenseRequiredColumns.All(col => properties.Contains(col));
    }

    public ValidationResult ValidateRevenueRow(RevenueInputRow row, string idNguoiDung, string nguoiNhap)
    {
        var result = new ValidationResult();
        var errors = new List<string>();

        // Validate ThangTaiChinh
        var thangTaiChinh = ValidateThangTaiChinh(row.ThangTaiChinh, errors);
        
        // Validate NamTaiChinh  
        var namTaiChinh = ValidateNamTaiChinh(row.NamTaiChinh, errors);
        
        // Validate TenNguon -> IdNguon
        var idNguon = ValidateAndMapNguon(row.TenNguon, errors);
        
        // Validate LoaiThu
        var loaiThu = ValidateLoaiThu(row.LoaiThu, errors);
        
        // Validate SoTien
        var soTien = ValidateSoTien(row.SoTien, errors);
        
        // Validate MoTa (optional)
        var moTa = ValidateMoTa(row.MoTa);
        
        // Validate GhiChu (optional)
        var ghiChu = ValidateGhiChu(row.GhiChu);

        // Set validation result
        result.IsValid = !errors.Any();
        result.Errors = errors;

        if (result.IsValid)
        {
            result.ValidatedRevenueRow = new RevenueDataRow
            {
                ThangTaiChinh = thangTaiChinh,
                NamTaiChinh = namTaiChinh,
                IdNguon = idNguon,
                LoaiThu = loaiThu,
                SoTien = soTien,
                MoTa = moTa,
                GhiChu = ghiChu,
                ThoiGianNhap = "",
                IDNguoiDung = idNguoiDung,
                NguoiNhap = nguoiNhap
            };
        }

        // Create Excel output row
        result.OutputRow = new ExcelOutputRow
        {
            ThangTaiChinh = row.ThangTaiChinh ?? "",
            NamTaiChinh = row.NamTaiChinh ?? "",
            TenNguon = EscapeJsonString(row.TenNguon ?? ""),
            LoaiThu = "Thu hoạt động",
            SoTien = row.SoTien ?? "",
            MoTa = EscapeJsonString(row.MoTa ?? ""),
            GhiChu = EscapeJsonString(row.GhiChu ?? ""),
            KetQuaXuLy = result.IsValid ? "Thành công" : string.Join("; ", errors)
        };

        return result;
    }

    public ValidationResult ValidateExpenseRow(ExpenseInputRow row, string idNguoiDung, string nguoiNhap)
    {
        var result = new ValidationResult();
        var errors = new List<string>();

        // Validate ThangTaiChinh
        var thangTaiChinh = ValidateThangTaiChinh(row.ThangTaiChinh, errors);
        
        // Validate NamTaiChinh  
        var namTaiChinh = ValidateNamTaiChinh(row.NamTaiChinh, errors);
        
        // Validate TenNguon -> IdNguon (for expense)
        var idNguon = ValidateAndMapExpenseNguon(row.TenNguon, errors);
        
        // Validate LoaiChi
        var loaiChi = ValidateLoaiChi(row.LoaiChi, errors);
        
        // Validate SoTien
        var soTien = ValidateSoTien(row.SoTien, errors);
        
        // Validate MoTa (optional)
        var moTa = ValidateMoTa(row.MoTa);
        
        // Validate GhiChu (optional)
        var ghiChu = ValidateGhiChu(row.GhiChu);

        // Set validation result
        result.IsValid = !errors.Any();
        result.Errors = errors;

        if (result.IsValid)
        {
            result.ValidatedExpenseRow = new Models.Expense.ExpenseDataRow
            {
                ThangTaiChinh = thangTaiChinh,
                NamTaiChinh = namTaiChinh,
                IdNguon = idNguon,
                LoaiChi = loaiChi,
                SoTien = soTien,
                MoTa = moTa,
                GhiChu = ghiChu,
                ThoiGianNhap = "",
                IDNguoiDung = idNguoiDung,
                NguoiNhap = nguoiNhap
            };
        }

        // Create Excel output row
        result.OutputRow = new ExcelOutputRow
        {
            ThangTaiChinh = row.ThangTaiChinh ?? "",
            NamTaiChinh = row.NamTaiChinh ?? "",
            TenNguon = EscapeJsonString(row.TenNguon ?? ""),
            LoaiThu = "Chi hoạt động",
            SoTien = row.SoTien ?? "",
            MoTa = EscapeJsonString(row.MoTa ?? ""),
            GhiChu = EscapeJsonString(row.GhiChu ?? ""),
            KetQuaXuLy = result.IsValid ? "Thành công" : string.Join("; ", errors)
        };

        return result;
    }

    private string ValidateThangTaiChinh(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
            return "0";

        if (int.TryParse(value.Trim(), out int thang))
        {
            if (thang >= 1 && thang <= 12)
                return thang.ToString();
            else
                errors.Add("Tháng tài chính phải từ 1-12");
        }
        else
        {
            errors.Add("Tháng tài chính không hợp lệ");
        }

        return "0";
    }

    private string ValidateNamTaiChinh(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
            return DateTime.Now.Year.ToString();

        if (int.TryParse(value.Trim(), out int nam))
        {
            if (nam >= 2020 && nam <= 2030)
                return nam.ToString();
            else
                errors.Add("Năm tài chính phải từ 2020-2030");
        }
        else
        {
            errors.Add("Năm tài chính không hợp lệ");
        }

        return DateTime.Now.Year.ToString();
    }

    private string ValidateAndMapNguon(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Tên nguồn không được để trống");
            return "";
        }

        var cleanedValue = value.Trim();
        
        if (_nguonMap.ContainsKey(cleanedValue))
        {
            return _nguonMap[cleanedValue].ToString();
        }
        else
        {
            errors.Add($"Tên nguồn '{cleanedValue}' không hợp lệ");
            return "";
        }
    }
    
    private string ValidateAndMapExpenseNguon(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Tên nguồn không được để trống");
            return "";
        }

        var cleanedValue = value.Trim();
        
        if (_expenseNguonMap.ContainsKey(cleanedValue))
        {
            return _expenseNguonMap[cleanedValue].ToString();
        }
        else
        {
            errors.Add($"Tên nguồn '{cleanedValue}' không hợp lệ");
            return "";
        }
    }

    private string ValidateLoaiThu(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Loại thu không được để trống");
            return "";
        }

        return value.Trim();
    }
    
    private string ValidateLoaiChi(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Loại chi không được để trống");
            return "";
        }

        return value.Trim();
    }

    private string ValidateSoTien(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Số tiền không được để trống");
            return "0";
        }

        // Remove commas and spaces
        var cleanedValue = value.Replace(",", "").Replace(" ", "").Trim();
        
        if (decimal.TryParse(cleanedValue, out decimal soTien))
        {
            if (soTien > 0)
                return soTien.ToString();
            else
                errors.Add("Số tiền phải lớn hơn 0");
        }
        else
        {
            errors.Add("Số tiền không hợp lệ");
        }

        return "0";
    }

    private string ValidateMoTa(string? value)
    {
        return string.IsNullOrEmpty(value) ? "" : value.Trim();
    }

    private string ValidateGhiChu(string? value)
    {
        return string.IsNullOrEmpty(value) ? "" : value.Trim();
    }

    private string EscapeJsonString(string value)
    {
        return value.Replace("\"", "\\\"");
    }
}
