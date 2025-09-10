namespace DataProcessingAPI.Application.DTOs;

/// <summary>
/// Speed Test Result DTO - For Service Layer (INTERNAL)
/// </summary>
public class SpeedTestDto
{
    public long Id { get; set; }
    public DateTime ThoiGianDo { get; set; }
    public string DiaDiem { get; set; } = string.Empty;
    public double TocDoTaiXuong_Mbps { get; set; }
    public double TocDoTaiLen_Mbps { get; set; }
    public int Ping_ms { get; set; }
    public string NguoiNhap { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
}

/// <summary>
/// Speed Test Request DTO - For CREATE and UPDATE operations
/// </summary>
public class SpeedTestRequest
{
    public DateTime ThoiGianDo { get; set; }
    public string DiaDiem { get; set; } = string.Empty;
    public double TocDoTaiXuong_Mbps { get; set; }
    public double TocDoTaiLen_Mbps { get; set; }
    public int Ping_ms { get; set; }
    public string NguoiNhap { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
}

/// <summary>
/// Speed Test Response DTO - For API responses (GET operations)
/// </summary>
public class SpeedTestResponse
{
    public long Id { get; set; }
    public DateTime ThoiGianDo { get; set; }
    public string DiaDiem { get; set; } = string.Empty;
    public double TocDoTaiXuong_Mbps { get; set; }
    public double TocDoTaiLen_Mbps { get; set; }
    public int Ping_ms { get; set; }
    public string NguoiNhap { get; set; } = string.Empty;
    public DateTime ThoiGianNhap { get; set; }
    public string IDNguoiDung { get; set; } = string.Empty;
}

/// <summary>
/// Speed Test Import DTO - For Excel/bulk import (string format)
/// </summary>
public class SpeedTestImportDto
{
    public string ThoiGianDo { get; set; } = string.Empty;
    public string DiaDiem { get; set; } = string.Empty;
    public string TocDoTaiXuong_Mbps { get; set; } = string.Empty;
    public string TocDoTaiLen_Mbps { get; set; } = string.Empty;
    public string Ping_ms { get; set; } = string.Empty;
    public string NguoiNhap { get; set; } = string.Empty;
    public string IDNguoiDung { get; set; } = string.Empty;
}
