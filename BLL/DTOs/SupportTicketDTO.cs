using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class SupportTicketDTO
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // PENDING, PROCESSING, RESOLVED, CLOSED
        public DateTime? CreatedAt { get; set; }

        // Thông tin tiểu thương lấy từ bảng Users thông qua VendorProfiles
        public string VendorName { get; set; }
        public string VendorPhone { get; set; }

        // Danh sách ảnh từ bảng ticket_images
        public List<string> Images { get; set; } = new List<string>();
    }
}
