using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Vendor")]
    public class VendorProfilesController : ControllerBase
    {
        private readonly IVendorProfileService _vendorProfileService;

        public VendorProfilesController(IVendorProfileService vendorProfileService)
        {
            _vendorProfileService = vendorProfileService;
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var profile = await _vendorProfileService.GetProfileAsync(vendorId);
            if (profile == null) return NotFound(new { Message = "Không tìm thấy hồ sơ tiểu thương." });

            return Ok(profile);
        }

        [HttpPut("description")]
        public async Task<IActionResult> UpdateProfile([FromForm] VendorProfileUpdateRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var result = await _vendorProfileService.UpdateDescriptionAsync(vendorId, request);
            if (!result) return NotFound(new { Message = "Không tìm thấy hồ sơ tiểu thương." });

            return Ok(new { Success = true, Message = "Cập nhật mô tả gian hàng thành công!" });
        }

        private int GetVendorIdFromToken()
        {
            //var claim = User.Claims.FirstOrDefault(c => c.Type == "VendorId");
            //return claim != null ? int.Parse(claim.Value) : 0;
            return 4;
        }
    }
}
