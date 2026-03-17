using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("vendor_id")]
        public int VendorId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("product_name")]
        public string ProductName { get; set; } = null!;

        [MaxLength(50)]
        [Column("unit")]
        public string? Unit { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("VendorId")]
        public virtual VendorProfile? Vendor { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductCategory? Category { get; set; }

        // OData Navigation: Lấy lịch sử giá của sản phẩm
        public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }
}
