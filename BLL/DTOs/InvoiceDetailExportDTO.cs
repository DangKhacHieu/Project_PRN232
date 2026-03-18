using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class InvoiceDetailExportDTO
    {
        public int InvoiceId { get; set; }
        public string BusinessName { get; set; } = null!;
        public string StallCode { get; set; } = null!;
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;

        // Danh sách các mục phí (Tiền điện, nước, rác...)
        public List<InvoiceItemExportDTO> Items { get; set; } = new List<InvoiceItemExportDTO>();
    }
}
