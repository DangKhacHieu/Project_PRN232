using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class InvoiceResponseDTO
    {
        public int InvoiceId { get; set; }
        public int ContractId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal TotalAmount { get; set; } // Sẽ xử lý null ở Service
        public string Status { get; set; } = null!; // "UNPAID", "PAID", "OVERDUE"
        public DateTime? CreatedAt { get; set; }
    }
}
