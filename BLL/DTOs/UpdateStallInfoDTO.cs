using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class UpdateStallInfoDTO
	{
		public string StallCode { get; set; } = string.Empty;
		public double Width { get; set; }
		public double Height { get; set; }
		public string? AllowedBusinessType { get; set; }
		public string Status { get; set; } = string.Empty; // VACANT, RENTED, MAINTENANCE
	}
}
