using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_BE.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
	public class VendorsController : ControllerBase
	{
		private readonly IVendorService _vendorService;

		public VendorsController(IVendorService vendorService)
		{
			_vendorService = vendorService;
		}

		// GET: api/vendors
		[HttpGet]
		public async Task<IActionResult> GetAllVendors()
		{
			try
			{
				var vendors = await _vendorService.GetAllVendorsAsync();
				return Ok(vendors);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		// DELETE: api/vendors/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteVendor(int id)
		{
			try
			{
				await _vendorService.DeleteVendorAsync(id);
				return Ok(new { message = "Đã xóa tiểu thương thành công!" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}
