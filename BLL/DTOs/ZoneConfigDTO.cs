using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class ZoneConfigDTO
	{
		public string ZonePrefix { get; set; } = null!; // Tiền tố mã sạp (VD: A, B, TP)
		public string ZoneName { get; set; } = null!;
		public int NumberOfStalls { get; set; }
		public int Columns { get; set; }
		public double StallWidth { get; set; }
		public double StallHeight { get; set; }
		public double StallGap { get; set; }
		public string? AllowedBusinessType { get; set; }
	}
}
