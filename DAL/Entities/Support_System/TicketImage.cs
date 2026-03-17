using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities.Support_System
{
    [Table("ticket_images")]
    public class TicketImage
    {
        [Key]
        [Column("image_id")]
        public int ImageId { get; set; }

        [Required]
        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [ForeignKey("TicketId")]
        public virtual SupportTicket? Ticket { get; set; }
    }
}
