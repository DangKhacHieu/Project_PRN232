using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("fee_config")]
    public class FeeConfig
    {
        [Key]
        [Column("fee_id")]
        public int FeeId { get; set; }

        [Required]
        [Column("fee_type_id")]
        public int FeeTypeId { get; set; }

        [Required]
        [Column("unit_price", TypeName = "decimal(14, 2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column("effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [ForeignKey("FeeTypeId")]
        public virtual FeeType? FeeType { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
