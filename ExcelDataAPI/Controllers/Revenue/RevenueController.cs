using Microsoft.AspNetCore.Mvc;
using ExcelDataAPI.Models.Revenue;
using ExcelDataAPI.Models.Common;
using ExcelDataAPI.Services;

namespace ExcelDataAPI.Controllers.Revenue
{
    [ApiController]
    [Route("api/revenue")]
    public class RevenueController : ControllerBase
    {
        private readonly ValidationService _validationService;
        private readonly DataService _dataService;

        public RevenueController(ValidationService validationService, DataService dataService)
        {
            _validationService = validationService;
            _dataService = dataService;
        }

        /// <summary>
        /// Import Excel revenue data (Thu hoạt động)
        /// </summary>
        [HttpPost("import")]
        public async Task<IActionResult> ImportRevenueData([FromBody] RevenueImportRequest request)
        {
            if (request?.Data == null || !request.Data.Any())
            {
                return BadRequest(new ImportResponse 
                { 
                    Success = false, 
                    Message = "Không có dữ liệu thu để xử lý" 
                });
            }

            try
            {
                var response = new ImportResponse
                {
                    TotalRows = request.Data.Count
                };

                // Validate từng row
                var validRows = new List<RevenueDataRow>();
                var validationResults = new List<ValidationResult>();

                foreach (var row in request.Data)
                {
                    var validation = _validationService.ValidateRevenueRow(row, request.IDNguoiDung, request.NguoiNhap);
                    validationResults.Add(validation);

                    if (validation.IsValid && validation.ValidatedRevenueRow != null)
                    {
                        validRows.Add(validation.ValidatedRevenueRow);
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
                    response.Message = "Không có dữ liệu hợp lệ để import";
                    return BadRequest(response);
                }

                // Bulk insert valid rows
                var insertResult = await _dataService.BulkInsertRevenueDataAsync(validRows);
                
                response.Success = insertResult.Success;
                response.InsertedRows = insertResult.InsertedRows;
                response.Message = insertResult.Success 
                    ? $"Import thu thành công {insertResult.InsertedRows}/{response.TotalRows} rows"
                    : $"Import thu thất bại: {insertResult.Message}";
                response.ErrorDetail = insertResult.Success ? null : insertResult.Message;

                return insertResult.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ImportResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống khi xử lý dữ liệu thu",
                    ErrorDetail = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check cho revenue API
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Revenue API đang hoạt động", timestamp = DateTime.UtcNow });
        }
    }
}
