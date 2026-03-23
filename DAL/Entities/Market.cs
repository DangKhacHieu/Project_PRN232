using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("markets")]
    public class Market
    {
        [Key]
        [Column("market_id")]
        public int MarketId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("market_name")]
        public string MarketName { get; set; } = null!;

        [Column("address")]
        public string? Address { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }



}
