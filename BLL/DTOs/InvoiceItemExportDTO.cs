using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class InvoiceItemExportDTO
    {
        public string FeeName { get; set; } = null!; // Lấy từ bảng fee_types
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
