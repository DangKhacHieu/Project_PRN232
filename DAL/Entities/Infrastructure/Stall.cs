using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Vendor_Contract;

namespace DAL.Entities.Infrastructure
{
    [Table("stalls")]
    public class Stall
    {
        [Key]
        [Column("stall_id")]
        public int StallId { get; set; }

        [Required]
        [Column("zone_id")]
        public int ZoneId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("stall_code")]
        public string StallCode { get; set; } = null!;

        [Column("area_m2")]
        public decimal? AreaM2 { get; set; }

        [MaxLength(150)]
        [Column("allowed_business_type")]
        public string? AllowedBusinessType { get; set; }

        // "VACANT", "RENTED", "MAINTENANCE", "DISPUTE"
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "VACANT";

        [Column("pos_x")]
        public double? PosX { get; set; }

        [Column("pos_y")]
        public double? PosY { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ZoneId")]
        public virtual Zone? Zone { get; set; }
        public virtual ICollection<StallContract> StallContracts { get; set; } = new List<StallContract>();
    }
}
