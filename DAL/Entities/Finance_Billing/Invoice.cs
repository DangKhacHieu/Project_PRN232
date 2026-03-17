using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Vendor_Contract;
using MarketManagement_DAL.Entities.Fee_Invoice_Items;

namespace DAL.Entities.Finance_Billing
{
    [Table("invoices")]
    public class Invoice
    {
        [Key]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Required]
        [Column("contract_id")]
        public int ContractId { get; set; }

        [Range(1, 12)]
        [Column("month")]
        public int? Month { get; set; }

        [Column("year")]
        public int? Year { get; set; }

        [Column("total_amount")]
        public decimal? TotalAmount { get; set; }

        // "UNPAID", "PAID", "OVERDUE"
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "UNPAID";

        [Column("qr_code_data")]
        public string? QrCodeData { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ContractId")]
        public virtual StallContract? Contract { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
