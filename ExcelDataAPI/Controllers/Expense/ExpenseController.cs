using Microsoft.AspNetCore.Mvc;
using ExcelDataAPI.Models.Expense;
using ExcelDataAPI.Models.Common;
using ExcelDataAPI.Services;

namespace ExcelDataAPI.Controllers.Expense
{
    [ApiController]
    [Route("api/expense")]
    public class ExpenseController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DataService _dataService;

        public ExpenseController(ValidationService validationService, DataService dataService)
        {
            _validationService = validationService;
            _dataService = dataService;
        }

        /// <summary>
        /// Import Excel expense data (Chi hoạt động)
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportExpenseData([FromBody] ExpenseImportRequest request)
        {
            if (request?.Data == null || !request.Data.Any())
            {
                return BadRequest(new ImportResponse 
                { 
                    Success = false, 
                    Message = "Không có dữ liệu chi để xử lý" 
                });
            }

            try
            {
                var response = new ImportResponse
                {
                    TotalRows = request.Data.Count
                };

                // Validate từng row
                var validRows = new List<ExpenseDataRow>();
                var validationResults = new List<ValidationResult>();

                foreach (var row in request.Data)
                {
                    var validation = _validationService.ValidateExpenseRow(row, request.IDNguoiDung, request.NguoiNhap);
                    validationResults.Add(validation);

                    if (validation.IsValid && validation.ValidatedExpenseRow != null)
                    {
                        validRows.Add(validation.ValidatedExpenseRow);
                    }
                }

                response.ValidRows = validRows.Count;
                response.InvalidRows = response.TotalRows - response.ValidRows;
                response.ValidationDetails = validationResults;
                response.ExcelOutputData = validationResults.Select(v => v.OutputRow!).ToList();

                if (response.ValidRows == 0)
                {
                    response.Success = false;
                    response.Message = "Không có dữ liệu hợp lệ để import";
                    return BadRequest(response);
                }

                // Insert data to TaiChinh_ChiHoatDong table
                var insertResult = await _dataService.BulkInsertExpenseDataAsync(validRows);
                
                response.Success = insertResult.Success;
                response.InsertedRows = insertResult.InsertedRows;
                response.Message = insertResult.Success 
                    ? $"Import chi thành công {insertResult.InsertedRows}/{response.TotalRows} rows"
                    : $"Import chi thất bại: {insertResult.Message}";
                response.ErrorDetail = insertResult.Success ? null : insertResult.Message;

                return insertResult.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi xử lý dữ liệu chi",
                    ErrorDetail = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check cho expense API
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Expense API đang hoạt động", timestamp = DateTime.UtcNow });
        }
    }
}
