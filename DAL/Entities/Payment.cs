using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Required]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        // "QR", "CASH", "BANK_TRANSFER"
        [MaxLength(30)]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [MaxLength(150)]
        [Column("transaction_code")]
        public string? TransactionCode { get; set; }

        [Column("paid_amount")]
        public decimal? PaidAmount { get; set; }

        // "PENDING", "SUCCESS", "FAILED", "REFUNDED", "CANCELLED"
        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("InvoiceId")]
        public virtual Invoice? Invoice { get; set; }
    }
}
