using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class MarketDTO
    {
        public int MarketId { get; set; }
        public string? MarketName { get; set; }
        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
