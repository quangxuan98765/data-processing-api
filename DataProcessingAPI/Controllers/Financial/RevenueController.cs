using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Controllers.Base;

namespace DataProcessingAPI.Controllers.Financial;

/// <summary>
/// Revenue Controller - EXCEL IMPORT + CRUD VỚI STORED PROCEDURES
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiExplorerSettings(GroupName = "financial")]
public class RevenueController : BaseApiController
{
    private readonly IRevenueService _revenueService;

    public RevenueController(IRevenueService revenueService, ILogger<RevenueController> logger)
        : base(logger)
    {
        _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
    }

    /// <summary>📊 BULK IMPORT REVENUE FROM EXCEL</summary>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport([FromBody] List<RevenueImportDto> data)
    {
        if (data == null || !data.Any())
        {
            _logger.LogWarning("Bulk import attempted with no data");
            return HandleError("Không có dữ liệu để import", 400);
        }

        try
        {
            _logger.LogInformation("Starting bulk import for {Count} revenue records", data.Count);
            var result = await _revenueService.BulkInsertAsync(data);
            
            if (result.Success)
            {
                _logger.LogInformation("Bulk import completed successfully");
                return HandleSuccess(result, "Bulk import successful");
            }

            _logger.LogWarning("Bulk import failed: {Message}", result.Message);
            return HandleError(result.Message, 400);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import operation");
            return HandleError("An error occurred while processing your request");
        }
    }

    /// <summary>📋 GET ALL REVENUE</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await ExecuteAsync(() => _revenueService.GetAllAsync(), "get all revenues", "Data retrieved successfully");
    }

    /// <summary>🔍 GET REVENUE BY ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var revenue = await _revenueService.GetByIdAsync(id);
            if (revenue == null)
            {
                _logger.LogWarning("Revenue with ID {Id} not found", id);
                throw new KeyNotFoundException($"Revenue with ID {id} not found");
            }
            return revenue;
        }, $"get revenue {id}", "Data retrieved successfully");
    }

    /// <summary>➕ CREATE REVENUE</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRevenueRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert CreateRevenueRequest to RevenueDto for service
        var revenue = new RevenueDto
        {
            ThangTaiChinh = request.ThangTaiChinh,
            NamTaiChinh = request.NamTaiChinh,
            IdNguon = request.IdNguon,
            LoaiThu = request.LoaiThu,
            SoTien = request.SoTien,
            MoTa = request.MoTa,
            GhiChu = request.GhiChu,
            IDNguoiDung = request.IDNguoiDung,
            NguoiNhap = request.NguoiNhap,
            ThoiGianNhap = DateTime.Now // Set automatically
        };

        return await ExecuteAsync(
            () => _revenueService.CreateAsync(revenue),
            "create revenue",
            "Revenue created successfully",
            201
        );
    }

    /// <summary>✏️ UPDATE REVENUE</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRevenueRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert UpdateRevenueRequest to RevenueDto for service
        var revenue = new RevenueDto
        {
            Id = id,
            ThangTaiChinh = request.ThangTaiChinh,
            NamTaiChinh = request.NamTaiChinh,
            IdNguon = request.IdNguon,
            LoaiThu = request.LoaiThu,
            SoTien = request.SoTien,
            MoTa = request.MoTa,
            GhiChu = request.GhiChu,
            IDNguoiDung = request.IDNguoiDung,
            NguoiNhap = request.NguoiNhap
            // ThoiGianNhap will be preserved by service
        };

        return await ExecuteAsync(
            () => _revenueService.UpdateAsync(id, revenue),
            $"update revenue {id}",
            "Revenue updated successfully"
        );
    }

    /// <summary>❌ DELETE REVENUE</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await ExecuteAsync(
            () => _revenueService.DeleteAsync(id),
            $"delete revenue {id}",
            "Revenue deleted successfully"
        );
    }
}
