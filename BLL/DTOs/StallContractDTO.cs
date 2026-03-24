using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class StallContractDTO
    {
        // 1. Các trường dùng để POST về API
        [Display(Name = "Mã hợp đồng")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn sạp")]
        [Display(Name = "Sạp")]
        public int StallId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tiểu thương")]
        [Display(Name = "Tiểu thương")]
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(6);

        [Required(ErrorMessage = "Vui lòng nhập giá thuê")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải là số dương")]
        [Display(Name = "Giá thuê tháng")]
        public decimal MonthlyRent { get; set; }

        [Display(Name = "Tiền đặt cọc")]
        public decimal? DepositAmount { get; set; }

        [Display(Name = "Trạng thái")]
        public string? Status { get; set; } // ACTIVE, EXPIRED, TERMINATED

        public DateTime? CreatedAt { get; set; }

        // 2. Các trường dùng để hiển thị lên Table (Mapping từ Join SQL)
        // Những trường này thường chỉ dùng để Read (GET)
        public string? StallCode { get; set; }
        public string? BusinessName { get; set; } // Tên cửa hàng của tiểu thương
        public string? VendorName { get; set; }   // Tên thật của tiểu thương
    }


}
