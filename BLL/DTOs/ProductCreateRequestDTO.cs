using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ProductCreateRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [MaxLength(200)]
        public required string ProductName { get; set; }

        public required string Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        public decimal Price { get; set; } // Giá sẽ được lưu vào bảng PriceHistory

        //// IFormFile dùng để nhận file ảnh từ Multipart/FormData
        //public IFormFile? Image { get; set; }
    }
}
