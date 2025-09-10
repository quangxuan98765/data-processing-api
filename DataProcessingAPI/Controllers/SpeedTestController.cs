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

    /// <summary>
    /// Get all speed test results with optional date filtering
    /// </summary>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <returns>List of speed test results</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        return await ExecuteAsync(async () =>
        {
            var speedTests = await _speedTestService.GetSpeedTestResultsAsync(startDate, endDate);
            
            var response = speedTests.Select(st => new SpeedTestResponse
            {
                Id = st.Id,
                ThoiGianDo = st.ThoiGianDo,
                DiaDiem = st.DiaDiem,
                TocDoTaiXuong_Mbps = st.TocDoTaiXuong_Mbps,
                TocDoTaiLen_Mbps = st.TocDoTaiLen_Mbps,
                Ping_ms = st.Ping_ms,
                NguoiNhap = st.NguoiNhap,
                ThoiGianNhap = st.ThoiGianNhap,
                IDNguoiDung = st.IDNguoiDung
            }).ToList();

            return response;
        }, "get all speed test results", "Speed test results retrieved successfully");
    }

    /// <summary>
    /// Get speed test result by ID
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <returns>Speed test result</returns>
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

            var response = new SpeedTestResponse
            {
                Id = speedTest.Id,
                ThoiGianDo = speedTest.ThoiGianDo,
                DiaDiem = speedTest.DiaDiem,
                TocDoTaiXuong_Mbps = speedTest.TocDoTaiXuong_Mbps,
                TocDoTaiLen_Mbps = speedTest.TocDoTaiLen_Mbps,
                Ping_ms = speedTest.Ping_ms,
                NguoiNhap = speedTest.NguoiNhap,
                ThoiGianNhap = speedTest.ThoiGianNhap,
                IDNguoiDung = speedTest.IDNguoiDung
            };

            return response;
        }, $"get speed test {id}", "Speed test retrieved successfully");
    }

    /// <summary>
    /// Create new speed test result
    /// </summary>
    /// <param name="request">Speed test data</param>
    /// <returns>Created speed test result with ID</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SpeedTestRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        return await ExecuteAsync(async () =>
        {
            var speedTestDto = new SpeedTestDto
            {
                ThoiGianDo = request.ThoiGianDo,
                DiaDiem = request.DiaDiem,
                TocDoTaiXuong_Mbps = request.TocDoTaiXuong_Mbps,
                TocDoTaiLen_Mbps = request.TocDoTaiLen_Mbps,
                Ping_ms = request.Ping_ms,
                NguoiNhap = request.NguoiNhap,
                IDNguoiDung = request.IDNguoiDung
            };

            var newId = await _speedTestService.CreateSpeedTestAsync(speedTestDto);
            if (newId <= 0)
            {
                throw new InvalidOperationException("Failed to create speed test result");
            }

            _logger.LogInformation("Speed test created with ID {Id}", newId);
            return new { id = newId };
        }, "create speed test", "Speed test created successfully", 201);
    }

    /// <summary>
    /// Update existing speed test result
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <param name="request">Updated speed test data</param>
    /// <returns>Success or error message</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] SpeedTestRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        return await ExecuteAsync(async () =>
        {
            var speedTestDto = new SpeedTestDto
            {
                Id = id,
                ThoiGianDo = request.ThoiGianDo,
                DiaDiem = request.DiaDiem,
                TocDoTaiXuong_Mbps = request.TocDoTaiXuong_Mbps,
                TocDoTaiLen_Mbps = request.TocDoTaiLen_Mbps,
                Ping_ms = request.Ping_ms,
                NguoiNhap = request.NguoiNhap,
                IDNguoiDung = request.IDNguoiDung
            };

            var success = await _speedTestService.UpdateSpeedTestAsync(id, speedTestDto);
            if (!success)
            {
                throw new InvalidOperationException("Failed to update speed test result or insufficient permissions");
            }

            _logger.LogInformation("Speed test {Id} updated successfully", id);
            return new { id = id };
        }, $"update speed test {id}", "Speed test updated successfully");
    }

    /// <summary>
    /// Delete speed test result
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await ExecuteAsync(async () =>
        {
            // Lấy userId từ JWT token thay vì query parameter
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            var userId = userIdClaim.Value;
            var success = await _speedTestService.DeleteSpeedTestAsync(id, userId);
            if (!success)
            {
                throw new InvalidOperationException("Failed to delete speed test result or insufficient permissions");
            }

            _logger.LogInformation("Speed test {Id} deleted successfully", id);
            return new { id = id };
        }, $"delete speed test {id}", "Speed test deleted successfully");
    }
}
