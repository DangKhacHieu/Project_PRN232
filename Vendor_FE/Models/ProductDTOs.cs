using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Vendor_FE.Models
{
    public class ProductResponseDTO
    {
        public int ProductId { get; set; }
        public int VendorId { get; set; }
        public int? CategoryId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CategoryName { get; set; }
    }

    public class ProductCreateRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        public required string ProductName { get; set; }

        public required string Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        public decimal Price { get; set; }
    }

    public class ProductUpdateRequestDTO
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; } = null!;

        public int? CategoryId { get; set; }

        public string? Unit { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}
