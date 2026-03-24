using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using BLL.Models;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorProfilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VendorProfilesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/vendorprofiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorProfileDTO>> GetVendorProfile(int id)
        {
            var vp = await _context.VendorProfiles
                                   .Include(v => v.User)
                                   .FirstOrDefaultAsync(v => v.VendorId == id);

            if (vp == null) return NotFound();

            var dto = new VendorProfileDTO
            {
                VendorId = vp.VendorId,
                UserId = vp.UserId,
                BusinessName = vp.BusinessName,
                Description = vp.Description,
                CoverImageUrl = vp.CoverImageUrl,
                FullName = vp.User?.FullName
            };

            return Ok(dto);
        }

        // PUT: api/vendorprofiles/5
        // Accept multipart/form-data with text fields and file field named "avatar"
        [HttpPut("{id}")]
        [RequestSizeLimit(10_000_000)] // optional: limit 10MB
        public async Task<IActionResult> UpdateVendorProfile(int id, [FromForm] VendorProfileUpdateModel model)
        {
            var vp = await _context.VendorProfiles.FirstOrDefaultAsync(v => v.VendorId == id);
            if (vp == null) return NotFound();

            // update textual fields
            if (!string.IsNullOrWhiteSpace(model.BusinessName)) vp.BusinessName = model.BusinessName;
            if (!string.IsNullOrWhiteSpace(model.Description)) vp.Description = model.Description;
            if (!string.IsNullOrWhiteSpace(model.FullName) && vp.User != null) vp.User.FullName = model.FullName;

            // handle avatar/cover upload
            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "vendors");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(model.Avatar.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(fs);
                }

                // set public URL path
                vp.CoverImageUrl = $"/uploads/vendors/{fileName}";
            }

            _context.Update(vp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Updated", cover = vp.CoverImageUrl });
        }

        public class VendorProfileUpdateModel
        {
            public string? BusinessName { get; set; }
            public string? Description { get; set; }
            public string? FullName { get; set; }
            public IFormFile? Avatar { get; set; } // field name expected: "avatar"
        }
    }
}