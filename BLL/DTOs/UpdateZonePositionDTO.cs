using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class UpdateZonePositionDTO
	{
		public int ZoneId { get; set; }
		public double MinX { get; set; }
		public double MinY { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
	}
}