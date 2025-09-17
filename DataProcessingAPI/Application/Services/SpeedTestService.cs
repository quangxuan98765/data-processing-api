using DataAccess;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataProcessingAPI.Application.Services;

public class SpeedTestService : ISpeedTestService
{
    private readonly IDatabaseService _databaseService;

    public SpeedTestService(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<List<SpeedTestDto>> GetSpeedTestResultsAsync(DateTime? startDate, DateTime? endDate)
    {
        var parameters = new 
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var dataTable = await _databaseService.ExecuteStoredProcAsync("sp_Get_ICT_SpeedTestResults", parameters);
        return ConvertDataTableToSpeedTestDtoList(dataTable);
    }

    public async Task<SpeedTestDto?> GetSpeedTestByIdAsync(long id)
    {
        var parameters = new 
        {
            StartDate = (DateTime?)null,
            EndDate = (DateTime?)null
        };

        var dataTable = await _databaseService.ExecuteStoredProcAsync("sp_Get_ICT_SpeedTestResults", parameters);
        var allResults = ConvertDataTableToSpeedTestDtoList(dataTable);
        
        return allResults.FirstOrDefault(r => r.Id == id);
    }

    public async Task<int> CreateSpeedTestAsync(SpeedTestDto speedTest)
    {
        var parameters = new 
        {
            ThoiGianDo = speedTest.ThoiGianDo,
            DiaDiem = speedTest.DiaDiem,
            TocDoTaiXuong_Mbps = speedTest.TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps = speedTest.TocDoTaiLen_Mbps,
            Ping_ms = speedTest.Ping_ms,
            NguoiNhap = speedTest.NguoiNhap,
            IDNguoiDung = speedTest.IDNguoiDung
        };

        var dataTable = await _databaseService.ExecuteStoredProcAsync("sp_Insert_ICT_SpeedTestResults", parameters);
        if (dataTable.Rows.Count > 0)
        {
            var returnCode = Convert.ToInt32(dataTable.Rows[0]["ReturnCode"]);
            return returnCode > 0 ? returnCode : 0; // Return the new ID if successful, 0 if failed
        }
        return 0; // Return 0 instead of -1
    }

    public async Task<int> UpdateSpeedTestAsync(long id, SpeedTestDto speedTest)
    {
        var parameters = new 
        {
            ID = id,
            ThoiGianDo = speedTest.ThoiGianDo,
            DiaDiem = speedTest.DiaDiem,
            TocDoTaiXuong_Mbps = speedTest.TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps = speedTest.TocDoTaiLen_Mbps,
            Ping_ms = speedTest.Ping_ms,
            NguoiNhap = speedTest.NguoiNhap,
            IDNguoiDung = speedTest.IDNguoiDung
        };

        var dataTable = await _databaseService.ExecuteStoredProcAsync("sp_Update_ICT_SpeedTestResults", parameters);
        if (dataTable.Rows.Count > 0)
        {
            var returnCode = Convert.ToInt32(dataTable.Rows[0]["ReturnCode"]);
            return returnCode > 0 ? (int)id : 0; // Return the ID if successful, 0 if failed
        }
        return 0; // Return 0 instead of false
    }

    public async Task<int> DeleteSpeedTestAsync(long id)
    {
        var parameters = new 
        {
            ID = id
        };

        var dataTable = await _databaseService.ExecuteStoredProcAsync("sp_Delete_ICT_SpeedTestResults", parameters);
        if (dataTable.Rows.Count > 0)
        {
            var returnCode = Convert.ToInt32(dataTable.Rows[0]["ReturnCode"]);
            return returnCode == 1 ? (int)id : 0; // Return the ID if successful, 0 if failed
        }
        return 0; // Return 0 instead of false
    }

    private List<SpeedTestDto> ConvertDataTableToSpeedTestDtoList(DataTable dataTable)
    {
        var speedTests = new List<SpeedTestDto>();

        foreach (DataRow row in dataTable.Rows)
        {
            speedTests.Add(new SpeedTestDto
            {
                Id = Convert.ToInt64(row["ID"]),
                ThoiGianDo = Convert.ToDateTime(row["ThoiGianDo"]),
                DiaDiem = row["DiaDiem"].ToString() ?? string.Empty,
                TocDoTaiXuong_Mbps = Convert.ToDouble(row["TocDoTaiXuong_Mbps"]),
                TocDoTaiLen_Mbps = Convert.ToDouble(row["TocDoTaiLen_Mbps"]),
                Ping_ms = Convert.ToInt32(row["Ping_ms"]),
                NguoiNhap = row["NguoiNhap"].ToString() ?? string.Empty,
                ThoiGianNhap = Convert.ToDateTime(row["ThoiGianNhap"]),
                IDNguoiDung = row["IDNguoiDung"].ToString() ?? string.Empty
            });
        }

        return speedTests;
    }
}
