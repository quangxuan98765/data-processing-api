using Microsoft.AspNetCore.Mvc;
using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;

namespace DataProcessingAPI.Controllers.Financial;

/// <summary>
/// Revenue Controller - EXCEL IMPORT + CRUD VỚI STORED PROCEDURES
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "financial")]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenueService;

    public RevenueController(IRevenueService revenueService)
    {
        _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
    }

    /// <summary>📊 BULK IMPORT REVENUE FROM EXCEL</summary>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport([FromBody] List<RevenueImportDto> data)
    {
        if (data == null || !data.Any())
            return BadRequest("Không có dữ liệu để import");

        var result = await _revenueService.BulkInsertAsync(data);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>📋 GET ALL REVENUE</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var revenues = await _revenueService.GetAllAsync();
        return Ok(revenues);
    }

    /// <summary>🔍 GET REVENUE BY ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var revenue = await _revenueService.GetByIdAsync(id);
        return revenue != null ? Ok(revenue) : NotFound($"Revenue with ID {id} not found");
    }

    /// <summary>➕ CREATE REVENUE</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RevenueDto revenue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newId = await _revenueService.CreateAsync(revenue);
        
        return newId > 0 
            ? CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId, Message = "Created successfully" })
            : BadRequest("Failed to create revenue");
    }

    /// <summary>✏️ UPDATE REVENUE</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RevenueDto revenue)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _revenueService.UpdateAsync(id, revenue);
        
        return result > 0 
            ? Ok(new { Id = id, Message = "Updated successfully" })
            : BadRequest("Failed to update revenue");
    }

    /// <summary>❌ DELETE REVENUE</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _revenueService.DeleteAsync(id);
        
        return result > 0 
            ? Ok(new { Id = id, Message = "Deleted successfully" })
            : BadRequest("Failed to delete revenue");
    }
}
