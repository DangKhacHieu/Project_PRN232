using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Product_Management;
using DAL.Entities.System_Auth;

namespace DAL.Entities.Vendor_Contract
{
    [Table("vendor_profiles")]
    public class VendorProfile
    {
        [Key]
        [Column("vendor_id")]
        public int VendorId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [MaxLength(200)]
        [Column("business_name")]
        public string? BusinessName { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<StallContract> StallContracts { get; set; } = new List<StallContract>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
