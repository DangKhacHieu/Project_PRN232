using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class VendorDTO
	{
		// Mình dùng UserId làm VendorId luôn cho Frontend dễ gọi API sửa/xóa
		public int VendorId { get; set; }
		public string? BusinessName { get; set; }
		public DateTime? CreatedAt { get; set; }
		public VendorUserDetailDTO User { get; set; } = null!;
	}

	public class VendorUserDetailDTO
	{
		public string? FullName { get; set; }
		public string? Email { get; set; }
		public string? Phone { get; set; }
	}
}
