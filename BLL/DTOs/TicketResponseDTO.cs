using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class TicketResponseDTO
    {
        public int TicketId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        // Trả về danh sách URL ảnh để Frontend dễ dàng hiển thị
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
