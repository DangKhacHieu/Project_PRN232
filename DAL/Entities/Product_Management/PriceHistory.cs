using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities.Product_Management
{
    [Table("price_history")]
    public class PriceHistory
    {
        [Key]
        [Column("price_id")]
        public int PriceId { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("effective_time")]
        public DateTime? EffectiveTime { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
