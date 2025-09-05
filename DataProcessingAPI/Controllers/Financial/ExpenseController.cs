using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataProcessingAPI.Application.Interfaces.Financial;
using DataProcessingAPI.Application.DTOs;
using DataProcessingAPI.Controllers.Base;

namespace DataProcessingAPI.Controllers.Financial;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ApiExplorerSettings(GroupName = "financial")]
public class ExpenseController : BaseApiController
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService, ILogger<ExpenseController> logger)
        : base(logger)
    {
        _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
    }

    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport([FromBody] List<ExpenseImportDto> data)
    {
        if (data == null || !data.Any())
        {
            _logger.LogWarning("Bulk import attempted with no data");
            return HandleError("Không có dữ liệu để import", 400);
        }

        try
        {
            _logger.LogInformation("Starting bulk import for {Count} expense records", data.Count);
            var result = await _expenseService.BulkInsertAsync(data);
            
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await ExecuteAsync(() => _expenseService.GetAllAsync(), "get all expenses", "Data retrieved successfully");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var expense = await _expenseService.GetByIdAsync(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense with ID {Id} not found", id);
                throw new KeyNotFoundException($"Expense with ID {id} not found");
            }
            return expense;
        }, $"get expense {id}", "Data retrieved successfully");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert CreateExpenseRequest to ExpenseDto for service
        var expense = new ExpenseDto
        {
            ThangTaiChinh = request.ThangTaiChinh,
            NamTaiChinh = request.NamTaiChinh,
            IdNguon = request.IdNguon,
            LoaiChi = request.LoaiChi,
            SoTien = request.SoTien,
            MoTa = request.MoTa,
            GhiChu = request.GhiChu,
            IDNguoiDung = request.IDNguoiDung,
            NguoiNhap = request.NguoiNhap,
            ThoiGianNhap = DateTime.Now // Set automatically
        };

        return await ExecuteAsync(
            () => _expenseService.CreateAsync(expense),
            "create expense",
            "Expense created successfully",
            201
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseRequest request)
    {
        var validation = ValidateModel();
        if (validation != null) return validation;

        // Convert UpdateExpenseRequest to ExpenseDto for service
        var expense = new ExpenseDto
        {
            Id = id,
            ThangTaiChinh = request.ThangTaiChinh,
            NamTaiChinh = request.NamTaiChinh,
            IdNguon = request.IdNguon,
            LoaiChi = request.LoaiChi,
            SoTien = request.SoTien,
            MoTa = request.MoTa,
            GhiChu = request.GhiChu,
            IDNguoiDung = request.IDNguoiDung,
            NguoiNhap = request.NguoiNhap
            // ThoiGianNhap will be preserved by service
        };

        return await ExecuteAsync(
            () => _expenseService.UpdateAsync(id, expense),
            $"update expense {id}",
            "Expense updated successfully"
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await ExecuteAsync(
            () => _expenseService.DeleteAsync(id),
            $"delete expense {id}",
            "Expense deleted successfully"
        );
    }
}