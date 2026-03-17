using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BLL.DTOs
{
    public class ProductUpdateRequestDTO
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [MaxLength(200)]
        public string ProductName { get; set; } = null!;

        public int? CategoryId { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        // Cho phép upload ảnh mới, nếu null tức là giữ nguyên ảnh cũ
        public IFormFile? NewImage { get; set; }

        public bool IsActive { get; set; }
    }
}
