using BLL.DTOs;
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

        [HttpPost("{id}/reset-password-preview")]
        public async Task<IActionResult> PreviewResetPassword(int id)
        {
            try
            {
                var result = await _vendorService.PreviewAdminResetPasswordAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/confirm-reset-password")]
        public async Task<IActionResult> ConfirmResetPassword(int id, [FromBody] AdminConfirmResetPasswordDTO request)
        {
            try
            {
                await _vendorService.ConfirmAdminResetPasswordAsync(id, request.NewPassword);
                return Ok(new { message = "Cập nhật mật khẩu mới thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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
