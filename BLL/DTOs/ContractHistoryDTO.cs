using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class ContractHistoryDTO
    {
        [Display(Name = "Mã lịch sử")]
        public int HistoryId { get; set; }

        [Display(Name = "Mã hợp đồng")]
        public int? ContractId { get; set; }

        [Display(Name = "Loại tác động")]
        public string? ActionType { get; set; } // Ví dụ: RENEW, TERMINATE, CREATE

        [Display(Name = "Ngày thực hiện")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ActionDate { get; set; }

        [Display(Name = "Ghi chú/Lý do")]
        public string? Note { get; set; }

        // --- Các trường mở rộng để hiển thị lên giao diện (Mapping từ Join) ---

        [Display(Name = "Mã số sạp")]
        public string? StallCode { get; set; }

        [Display(Name = "Tên tiểu thương")]
        public string? BusinessName { get; set; }

        [Display(Name = "Người thực hiện")]
        public string? PerformedBy { get; set; } // Nếu bạn có lưu User thực hiện thao tác
    }
}
