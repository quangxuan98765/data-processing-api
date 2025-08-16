using Microsoft.AspNetCore.Mvc;
using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;

namespace DataProcessingAPI.Controllers.Financial;

/// <summary>
/// Expense Controller - EXCEL IMPORT + CRUD VỚI STORED PROCEDURES
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "financial")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
    }

    /// <summary>📊 BULK IMPORT EXPENSE FROM EXCEL</summary>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport([FromBody] List<ExpenseImportDto> data)
    {
        if (data == null || !data.Any())
            return BadRequest("Không có dữ liệu để import");

        var result = await _expenseService.BulkInsertAsync(data);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>📋 GET ALL EXPENSES</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var expenses = await _expenseService.GetAllAsync();
        return Ok(expenses);
    }

    /// <summary>🔍 GET EXPENSE BY ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        return expense != null ? Ok(expense) : NotFound($"Expense with ID {id} not found");
    }

    /// <summary>➕ CREATE EXPENSE</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExpenseDto expense)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newId = await _expenseService.CreateAsync(expense);
        
        return newId > 0 
            ? CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId, Message = "Created successfully" })
            : BadRequest("Failed to create expense");
    }

    /// <summary>✏️ UPDATE EXPENSE</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExpenseDto expense)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _expenseService.UpdateAsync(id, expense);
        
        return result > 0 
            ? Ok(new { Id = id, Message = "Updated successfully" })
            : BadRequest("Failed to update expense");
    }

    /// <summary>❌ DELETE EXPENSE</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _expenseService.DeleteAsync(id);
        
        return result > 0 
            ? Ok(new { Id = id, Message = "Deleted successfully" })
            : BadRequest("Failed to delete expense");
    }
}