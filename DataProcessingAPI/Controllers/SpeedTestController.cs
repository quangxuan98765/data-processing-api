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
public class SpeedTestController : BaseApiController
{
    private readonly ISpeedTestService _speedTestService;

    public SpeedTestController(ISpeedTestService speedTestService, ILogger<SpeedTestController> logger)
        : base(logger)
    {
        _speedTestService = speedTestService;
    }

    /// <summary>
    /// Get all speed test results with optional date filtering
    /// </summary>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <returns>List of speed test results</returns>
    [HttpGet]
    public async Task<IActionResult> GetSpeedTestResults(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
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

            return HandleSuccess(response);
        }
        catch (Exception)
        {
            return HandleError("Error retrieving speed test results");
        }
    }

    /// <summary>
    /// Get speed test result by ID
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <returns>Speed test result</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpeedTestById(long id)
    {
        try
        {
            var speedTest = await _speedTestService.GetSpeedTestByIdAsync(id);
            
            if (speedTest == null)
            {
                return HandleError($"Speed test with ID {id} not found", 404);
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

            return HandleSuccess(response);
        }
        catch (Exception)
        {
            return HandleError($"Error retrieving speed test with ID {id}");
        }
    }

    /// <summary>
    /// Create new speed test result
    /// </summary>
    /// <param name="request">Speed test data</param>
    /// <returns>Created speed test result with ID</returns>
    [HttpPost]
    public async Task<IActionResult> CreateSpeedTest([FromBody] SpeedTestRequest request)
    {
        try
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
                return HandleError("Failed to create speed test result", 400);
            }

            return HandleSuccess(new { id = newId }, "Speed test result created successfully");
        }
        catch (Exception)
        {
            return HandleError("Error creating speed test result");
        }
    }

    /// <summary>
    /// Update existing speed test result
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <param name="request">Updated speed test data</param>
    /// <returns>Success or error message</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpeedTest(long id, [FromBody] SpeedTestRequest request)
    {
        try
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
                return HandleError("Failed to update speed test result or insufficient permissions", 400);
            }

            return HandleSuccess(new { id = id }, "Speed test result updated successfully");
        }
        catch (Exception)
        {
            return HandleError($"Error updating speed test with ID {id}");
        }
    }

    /// <summary>
    /// Delete speed test result
    /// </summary>
    /// <param name="id">Speed test ID</param>
    /// <param name="userId">User ID for permission check</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpeedTest(long id, [FromQuery] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return HandleError("User ID is required", 400);
            }

            var success = await _speedTestService.DeleteSpeedTestAsync(id, userId);
            
            if (!success)
            {
                return HandleError("Failed to delete speed test result or insufficient permissions", 400);
            }

            return HandleSuccess(new { id = id }, "Speed test result deleted successfully");
        }
        catch (Exception)
        {
            return HandleError($"Error deleting speed test with ID {id}");
        }
    }
}
