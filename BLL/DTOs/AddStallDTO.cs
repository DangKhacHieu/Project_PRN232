using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class AddStallDTO
	{
		public int ZoneId { get; set; }
		public string StallCode { get; set; } = null!; // Mã sạp nhập tay, VD: "A-99"
		public double Width { get; set; }
		public double Height { get; set; }
		public double PosX { get; set; } // Tọa độ X ngay tại vị trí admin click chuột
		public double PosY { get; set; } // Tọa độ Y ngay tại vị trí admin click chuột
		public string? AllowedBusinessType { get; set; }
	}
}
