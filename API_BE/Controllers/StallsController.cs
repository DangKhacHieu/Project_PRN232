using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StallsController : ControllerBase
	{
		private readonly IMarketService _marketService;

		public StallsController(IMarketService marketService)
		{
			_marketService = marketService;
		}

		[HttpPost]
		public async Task<IActionResult> AddStall([FromBody] AddStallDTO request)
		{
			var result = await _marketService.AddSingleStallAsync(request);
			return Ok(new { Message = "Thêm sạp thành công", Stall = result });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteStall(int id)
		{
			await _marketService.DeleteStallAsync(id);
			return Ok(new { Message = "Đã xóa sạp" });
		}

		// NEW: GET api/stalls/{id} -> trả chi tiết sạp kèm hợp đồng/chủ (nếu có)
		[HttpGet("{id}")]
		public async Task<IActionResult> GetStallDetails(int id)
		{
			var stall = await _marketService.GetStallDetailsAsync(id);
			if (stall == null) return NotFound("Không tìm thấy sạp.");
			return Ok(stall);
		}
	}
}
