using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Finance_Billing;
using DAL.Entities.Infrastructure;

namespace DAL.Entities.Vendor_Contract
{
    [Table("stall_contracts")]
    public class StallContract
    {
        [Key]
        [Column("contract_id")]
        public int ContractId { get; set; }

        [Required]
        [Column("stall_id")]
        public int StallId { get; set; }

        [Required]
        [Column("vendor_id")]
        public int VendorId { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("monthly_rent")]
        public decimal MonthlyRent { get; set; }

        [Column("deposit_amount")]
        public decimal? DepositAmount { get; set; }

        // "ACTIVE", "EXPIRED", "TERMINATED"
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "ACTIVE";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("StallId")]
        public virtual Stall? Stall { get; set; }

        [ForeignKey("VendorId")]
        public virtual VendorProfile? Vendor { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
