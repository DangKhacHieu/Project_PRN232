using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin,Finance")]
    public class UtilityReadingsController : ControllerBase
    {
        private readonly IUtilityReadingService _utilityReadingService;

        public UtilityReadingsController(IUtilityReadingService utilityReadingService)
        {
            _utilityReadingService = utilityReadingService;
        }

        [HttpPost]
        public async Task<IActionResult> RecordUtility([FromBody] UtilityReadingInputDTO dto)
        {
            try
            {
                var result = await _utilityReadingService.RecordUtilityAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file, [FromQuery] int month, [FromQuery] int year)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = file.OpenReadStream();
                int recordsProcessed = await _utilityReadingService.ImportExcelAsync(stream, month, year);
                return Ok(new { Message = $"Nhập dữ liệu thành công. Đã xử lý {recordsProcessed} bản ghi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Lỗi khi đọc file Excel: {ex.Message}" });
            }
        }

        [HttpGet("all-stalls")]
        public async Task<IActionResult> GetAllStalls()
        {
            try
            {
                var stalls = await _utilityReadingService.GetAllStallsAsync();
                return Ok(stalls);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
