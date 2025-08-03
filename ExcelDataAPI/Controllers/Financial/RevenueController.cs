using Microsoft.AspNetCore.Mvc;
using ExcelDataAPI.Application.Interfaces.Financial;
using ExcelDataAPI.Application.DTOs;

namespace ExcelDataAPI.Controllers.Financial;

/// <summary>
/// Revenue Controller - CHỈ IMPORT EXCEL REVENUE
/// Import dữ liệu thu từ Excel file qua Power Automate
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenueService;

    public RevenueController(IRevenueService revenueService)
    {
        _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
    }

    /// <summary>
    /// Import Excel revenue data with built-in validation
    /// POST /api/revenue/import
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<BulkOperationResultDto>> Import([FromBody] FinancialImportRequestDto<RevenueImportDto> request)
    {
        if (request?.Data == null || !request.Data.Any())
        {
            return BadRequest(new BulkOperationResultDto 
            { 
                Success = false, 
                Message = "Không có dữ liệu revenue để xử lý" 
            });
        }

        try
        {
            // Set user info for all records
            foreach (var item in request.Data)
            {
                if (string.IsNullOrEmpty(item.IDNguoiDung))
                    item.IDNguoiDung = request.IdNguoiDung;
                if (string.IsNullOrEmpty(item.NguoiNhap))
                    item.NguoiNhap = request.NguoiNhap;
            }

            var result = await _revenueService.BulkInsertAsync(request.Data);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BulkOperationResultDto 
            { 
                Success = false, 
                Message = $"Revenue import server error: {ex.Message}" 
            });
        }
    }
}
