using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("support_tickets")]
    public class SupportTicket
    {
        [Key]
        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Required]
        [Column("vendor_id")]
        public int VendorId { get; set; }

        [MaxLength(200)]
        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(30)]
        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("VendorId")]
        public virtual VendorProfile? Vendor { get; set; }

        // OData Navigation: Lấy luôn danh sách ảnh đính kèm của sự cố
        public virtual ICollection<TicketImage> TicketImages { get; set; } = new List<TicketImage>();
    }
    public static class TicketStatus
    {
        public const string Pending = "PENDING";
        public const string Processing = "PROCESSING";
        public const string Resolved = "RESOLVED"; 
    }
}
