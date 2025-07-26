using ExcelDataAPI.Models;

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

    private readonly List<string> _requiredColumns = new()
    {
        "ThangTaiChinh", "NamTaiChinh", "TenNguon", "LoaiThu", "SoTien", "MoTa", "GhiChu"
    };

    public bool ValidateExcelStructure(List<ExcelInputRow> data)
    {
        if (!data.Any()) return false;

        var firstRow = data.First();
        var properties = typeof(ExcelInputRow).GetProperties().Select(p => p.Name).ToList();
        
        return _requiredColumns.All(col => properties.Contains(col));
    }

    public ValidationResult ValidateRow(ExcelInputRow row, string idNguoiDung, string nguoiNhap)
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
            result.ValidatedRow = new FinancialDataRow
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

    private string ValidateLoaiThu(string? value, List<string> errors)
    {
        if (string.IsNullOrEmpty(value))
        {
            errors.Add("Loại thu không được để trống");
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
