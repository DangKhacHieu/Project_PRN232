using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BLL.DTOs
{
    public class TicketCreateRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề sự cố.")]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết.")]
        public string Description { get; set; } = null!;

        // Cho phép upload 0, 1 hoặc nhiều ảnh cùng lúc
        public List<IFormFile>? Images { get; set; }
    }
}
