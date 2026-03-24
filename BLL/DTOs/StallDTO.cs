using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class StallDTO
    {
        public int StallId { get; set; }
        public string StallCode { get; set; }
        public decimal? AreaM2 { get; set; }
        public string Status { get; set; } // Để lọc VACANT
    }
}
