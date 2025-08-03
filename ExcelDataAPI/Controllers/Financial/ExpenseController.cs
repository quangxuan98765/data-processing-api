using Microsoft.AspNetCore.Mvc;
using ExcelDataAPI.Application.Interfaces.Financial;
using ExcelDataAPI.Application.DTOs;

namespace ExcelDataAPI.Controllers.Financial;

/// <summary>
/// Expense Controller - CHỈ IMPORT EXCEL EXPENSE  
/// Import dữ liệu chi từ Excel file qua Power Automate
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
    }

    /// <summary>
    /// Import Excel expense data with built-in validation
    /// POST /api/expense/import
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<BulkOperationResultDto>> Import([FromBody] FinancialImportRequestDto<ExpenseImportDto> request)
    {
        if (request?.Data == null || !request.Data.Any())
        {
            return BadRequest(new BulkOperationResultDto 
            { 
                Success = false, 
                Message = "Không có dữ liệu expense để xử lý" 
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

            var result = await _expenseService.BulkInsertAsync(request.Data);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BulkOperationResultDto 
            { 
                Success = false, 
                Message = $"Expense import server error: {ex.Message}" 
            });
        }
    }
}
