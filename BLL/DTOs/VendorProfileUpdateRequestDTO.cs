using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BLL.DTOs
{
    public class VendorProfileUpdateRequestDTO
    {
        [MaxLength(200)]
        public string? BusinessName { get; set; }

        public string? Description { get; set; }

        // Cho phép upload ảnh bìa mới
        public IFormFile? NewCoverImage { get; set; }
    }
}
