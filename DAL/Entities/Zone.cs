using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("zones")]
    public class Zone
    {
        [Key]
        [Column("zone_id")]
        public int ZoneId { get; set; }

        [Required]
        [Column("market_id")]
        public int MarketId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("zone_name")]
        public string ZoneName { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

		[Column("min_x")]
		public double? MinX { get; set; }

		[Column("min_y")]
		public double? MinY { get; set; }

		[Column("max_x")]
		public double? MaxX { get; set; }

		[Column("max_y")]
		public double? MaxY { get; set; }

		[ForeignKey("MarketId")]
        public virtual Market? Market { get; set; }
        public virtual ICollection<Stall> Stalls { get; set; } = new List<Stall>();
	}
}
