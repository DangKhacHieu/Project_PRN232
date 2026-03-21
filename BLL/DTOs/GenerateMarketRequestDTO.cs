using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	// DTO dùng cho Bước 1: Auto Generate
	public class GenerateMarketRequestDTO
	{
		public string MarketName { get; set; } = null!;
		public string? Address { get; set; }
		public double ZoneGap { get; set; } // Khoảng cách giữa các Khu
		public List<ZoneConfigDTO> Zones { get; set; } = new List<ZoneConfigDTO>();
	}
}
