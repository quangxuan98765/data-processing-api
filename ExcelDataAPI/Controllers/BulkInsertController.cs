using Microsoft.AspNetCore.Mvc;
using ExcelDataAPI.Models;
using ExcelDataAPI.Services;

namespace ExcelDataAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BulkInsertController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DataService _dataService;

        public BulkInsertController(ValidationService validationService, DataService dataService)
        {
            _validationService = validationService;
            _dataService = dataService;
        }

        /// <summary>
        /// API để bulk insert dữ liệu từ Power Automate với validation tích hợp
        /// </summary>
        [HttpPost("financial-data")]
        public async Task<IActionResult> BulkInsertFinancialData([FromBody] BulkInsertRequest request)
        {
            if (request?.Data == null || !request.Data.Any())
            {
                return BadRequest(new BulkInsertResponse 
                { 
                    Success = false, 
                    Message = "Không có dữ liệu để xử lý" 
                });
            }

            try
            {
                var response = new BulkInsertResponse
                {
                    TotalRows = request.Data.Count
                };

                // Validate Excel structure
                if (!_validationService.ValidateExcelStructure(request.Data))
                {
                    response.Success = false;
                    response.Message = "Cấu trúc file Excel không đúng. Thiếu các cột bắt buộc.";
                    return BadRequest(response);
                }

                // Validate từng row
                var validRows = new List<FinancialDataRow>();
                var validationResults = new List<ValidationResult>();

                foreach (var row in request.Data)
                {
                    var validation = _validationService.ValidateRow(row, request.IDNguoiDung, request.NguoiNhap);
                    validationResults.Add(validation);

                    if (validation.IsValid && validation.ValidatedRow != null)
                    {
                        validRows.Add(validation.ValidatedRow);
                    }
                }

                response.ValidRows = validRows.Count;
                response.InvalidRows = response.TotalRows - response.ValidRows;
                response.ValidationDetails = validationResults;
                response.ExcelOutputData = validationResults.Select(v => v.OutputRow!).ToList();

                // Nếu không có row nào hợp lệ
                if (!validRows.Any())
                {
                    response.Success = false;
                    response.Message = "Không có dữ liệu hợp lệ để insert";
                    return BadRequest(response);
                }

                // Bulk insert valid rows
                var insertResult = await _dataService.BulkInsertToSqlServer(validRows);
                
                response.Success = insertResult.Success;
                response.InsertedRows = insertResult.InsertedRows;
                response.Message = insertResult.Success 
                    ? $"Import thành công {insertResult.InsertedRows}/{response.TotalRows} rows"
                    : $"Import thất bại: {insertResult.Message}";
                response.ErrorDetail = insertResult.Success ? null : insertResult.Message;

                return insertResult.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BulkInsertResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi xử lý dữ liệu",
                    ErrorDetail = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "API đang hoạt động", timestamp = DateTime.UtcNow });
        }
    }
}
