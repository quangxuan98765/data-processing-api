using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Application.Interfaces;
using DataProcessingAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessingAPI.Controllers;

/// <summary>
/// Speed Test Results API Controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiExplorerSettings(GroupName = "speedtest")]
public class SpeedTestController : BaseApiController
{
    private readonly ISpeedTestService _speedTestService;

    public SpeedTestController(ISpeedTestService speedTestService, ILogger<SpeedTestController> logger)
        : base(logger)
    {
        _speedTestService = speedTestService ?? throw new ArgumentNullException(nameof(speedTestService));
    }

    /// <summary>📋 GET ALL SPEED TESTS</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await ExecuteAsync(() => _speedTestService.GetSpeedTestResultsAsync(startDate, endDate), "get all speed tests", "Data retrieved successfully");
    }

    /// <summary>🔍 GET SPEED TEST BY ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        return await ExecuteAsync(async () =>
        {
            var speedTest = await _speedTestService.GetSpeedTestByIdAsync(id);
            if (speedTest == null)
            {
                _logger.LogWarning("Speed test with ID {Id} not found", id);
                throw new KeyNotFoundException($"Speed test with ID {id} not found");
            }
            return speedTest;
        }, $"get speed test {id}", "Data retrieved successfully");
    }

    /// <summary>➕ CREATE SPEED TEST</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SpeedTestRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert SpeedTestRequest to SpeedTestDto for service
        var speedTest = new SpeedTestDto
        {
            ThoiGianDo = request.ThoiGianDo,
            DiaDiem = request.DiaDiem,
            TocDoTaiXuong_Mbps = request.TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps = request.TocDoTaiLen_Mbps,
            Ping_ms = request.Ping_ms,
            NguoiNhap = request.NguoiNhap,
            IDNguoiDung = request.IDNguoiDung,
            ThoiGianNhap = DateTime.Now // Set automatically
        };

        return await ExecuteAsync(
            () => _speedTestService.CreateSpeedTestAsync(speedTest),
            "create speed test",
            "Speed test created successfully",
            201
        );
    }

    /// <summary>✏️ UPDATE SPEED TEST</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] SpeedTestRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert SpeedTestRequest to SpeedTestDto for service
        var speedTest = new SpeedTestDto
        {
            Id = id,
            ThoiGianDo = request.ThoiGianDo,
            DiaDiem = request.DiaDiem,
            TocDoTaiXuong_Mbps = request.TocDoTaiXuong_Mbps,
            TocDoTaiLen_Mbps = request.TocDoTaiLen_Mbps,
            Ping_ms = request.Ping_ms,
            NguoiNhap = request.NguoiNhap,
            IDNguoiDung = request.IDNguoiDung
            // ThoiGianNhap will be preserved by service
        };

        return await ExecuteAsync(
            () => _speedTestService.UpdateSpeedTestAsync(id, speedTest),
            $"update speed test {id}",
            "Speed test updated successfully"
        );
    }

    /// <summary>❌ DELETE SPEED TEST</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await ExecuteAsync(async () =>
        {
            // 🔒 SpeedTest cần userId để check ownership (khác với Revenue/Expense)
            // Stored procedure sp_Delete_ICT_SpeedTestResults kiểm tra @OwnerID = @IDNguoiDung
            // Chỉ owner mới được xóa record của mình (security requirement)
            var userId = GetCurrentUserId().ToString();
            var success = await _speedTestService.DeleteSpeedTestAsync(id, userId);
            if (!success)
            {
                throw new InvalidOperationException("Failed to delete speed test result or insufficient permissions");
            }
            return new { id = id };
        }, $"delete speed test {id}", "Speed test deleted successfully");
    }
}
