using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ZoneDTO
    {
        public int ZoneId { get; set; }
        public int MarketId { get; set; }
        public string? ZoneName { get; set; }
        public string? Description { get; set; }

        // Thêm trường này nếu bạn muốn hiển thị tên chợ kèm theo khu vực
        public string? MarketName { get; set; }
    }
}
