using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class VendorEditViewModel
    {
        public int? VendorId { get; set; }
        public int? UserId { get; set; }

        public string? FullName { get; set; }
        public string? BusinessName { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        // file uploaded from browser
        public IFormFile? AvatarFile { get; set; }
    }
}