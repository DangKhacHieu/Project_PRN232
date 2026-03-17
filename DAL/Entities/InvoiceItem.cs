using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("invoice_items")]
    public class InvoiceItem
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Required]
        [Column("fee_id")]
        public int FeeId { get; set; }

        [Column("quantity")]
        public decimal? Quantity { get; set; }

        [Column("unit_price")]
        public decimal? UnitPrice { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }

        [ForeignKey("FeeId")]
        public virtual FeeConfig? FeeConfig { get; set; }
    }
}
