using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketManagement_DAL.Entities.Fee_Invoice_Items
{
    [Table("fee_types")]
    public class FeeType
    {
        [Key]
        [Column("fee_type_id")]
        public int FeeTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        public virtual ICollection<FeeConfig> FeeConfigs { get; set; } = new List<FeeConfig>();
    }
}
