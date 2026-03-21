using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BE.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MarketsController : ControllerBase
	{
		private readonly IMarketService _marketService;

		public MarketsController(IMarketService marketService)
		{
			_marketService = marketService;
		}

		// POST: api/markets/generate
		[HttpPost("generate")]
		public async Task<IActionResult> GenerateMarket([FromBody] GenerateMarketRequestDTO request)
		{
			try
			{
				if (request == null || !request.Zones.Any())
					return BadRequest("Dữ liệu đầu vào không hợp lệ hoặc thiếu thông tin Khu.");

				var result = await _marketService.GenerateAndSaveMarketAsync(request);

				// Trả về dữ liệu để FE có thể vẽ ngay sơ đồ mà không cần gọi API Get lại
				return Ok(new
				{
					Message = "Tạo sơ đồ chợ thành công!",
					MarketId = result.MarketId,
					MarketData = result
				});
			}
			// MỚI: BẮT LỖI TRÙNG TÊN CHỢ TỪ TẦNG SERVICE NÉM RA
			catch (ArgumentException ex)
			{
				// Trả về đúng format để đoạn JS (err.responseJSON.errors) tự động bật popup Alert
				return BadRequest(new { errors = new { MarketName = new[] { ex.Message } } });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		// PUT: api/markets/stalls/update-positions
		[HttpPut("stalls/update-positions")]
		public async Task<IActionResult> UpdateStallPositions([FromBody] List<UpdateStallPositionDTO> request)
		{
			try
			{
				if (request == null || !request.Any())
					return BadRequest("Không có dữ liệu vị trí cần cập nhật.");

				await _marketService.UpdateStallPositionsAsync(request);
				return Ok(new { Message = "Lưu vị trí mới thành công!" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		[HttpGet("{id}/layout")]
		public async Task<IActionResult> GetMarketLayout(int id)
		{
			var market = await _marketService.GetMarketLayoutAsync(id);
			if (market == null) return NotFound("Không tìm thấy chợ.");
			return Ok(market);
		}

		// ==========================================
		// API QUẢN LÝ SẠP LẺ (THÊM / SỬA / XÓA)
		// ==========================================

		// 1. THÊM SẠP LẺ
		[HttpPost("stalls")]
		public async Task<IActionResult> AddStall([FromBody] AddStallDTO request)
		{
			try
			{
				var newStall = await _marketService.AddSingleStallAsync(request);
				return Ok(new { message = "Thêm sạp thành công", stall = newStall });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		// 2. SỬA THÔNG TIN SẠP
		[HttpPut("stalls/{id}")]
		public async Task<IActionResult> UpdateStallInfo(int id, [FromBody] UpdateStallInfoDTO request)
		{
			try
			{
				await _marketService.UpdateStallInfoAsync(id, request);
				return Ok(new { message = "Cập nhật thông tin sạp thành công!" });
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		// 3. XÓA SẠP LẺ
		[HttpDelete("stalls/{id}")]
		public async Task<IActionResult> DeleteStall(int id)
		{
			try
			{
				await _marketService.DeleteStallAsync(id);
				return Ok(new { message = "Đã xóa sạp thành công!" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		// [GET] api/markets
		[HttpGet]
		public async Task<IActionResult> GetAllMarkets()
		{
			try
			{
				var markets = await _marketService.GetAllMarketsAsync();
				// Chỉ bóc tách ID và Tên chợ trả về cho Frontend để tối ưu tốc độ mạng
				var result = markets.Select(m => new {
					m.MarketId,
					m.MarketName
				});
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		// PUT: api/markets/zones/update-positions
		[HttpPut("zones/update-positions")]
		public async Task<IActionResult> UpdateZonePositions([FromBody] List<UpdateZonePositionDTO> request)
		{
			try
			{
				if (request == null || !request.Any())
					return BadRequest("Không có dữ liệu Khu cần cập nhật.");

				await _marketService.UpdateZonePositionsAsync(request);
				return Ok(new { Message = "Lưu thay đổi Khu thành công!" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		[HttpPost("{marketId}/zones")]
		public async Task<IActionResult> CreateZoneWithStalls(int marketId, [FromBody] CreateZoneWithStallsDTO request)
		{
			try
			{
				if (request == null) return BadRequest("Dữ liệu rỗng.");
				if (marketId != request.MarketId) request.MarketId = marketId;

				var created = await _marketService.AddZoneWithStallsAsync(request);
				return Ok(new { message = "Tạo khu thành công", zone = created });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}

		[HttpDelete("zones/{id}")]
		public async Task<IActionResult> DeleteZone(int id)
		{
			try
			{
				await _marketService.DeleteZoneAsync(id);
				return Ok(new { message = "Đã xóa khu thành công!" });
			}
			catch (KeyNotFoundException)
			{
				return NotFound("Không tìm thấy khu.");
			}
			catch (Exception ex)
			{
				// nếu có FK/constraint khác, trả message chi tiết để debug
				return StatusCode(500, $"Lỗi server: {ex.Message}");
			}
		}
	}
}
