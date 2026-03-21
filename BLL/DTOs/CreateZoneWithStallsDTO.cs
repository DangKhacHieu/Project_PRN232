using System.Collections.Generic;

namespace BLL.DTOs
{
	public class CreateZoneWithStallsDTO
	{
		public int MarketId { get; set; }
		public string ZoneName { get; set; } = string.Empty;
		// position & size of zone (meters)
		public double MinX { get; set; }
		public double MinY { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }

		// stall configuration
		public int NumberOfStalls { get; set; }
		public double StallWidth { get; set; }
		public double StallHeight { get; set; }
		public double StallGap { get; set; } = 0.5; // default gap (m)
		public string StallPrefix { get; set; } = "S";
	}
}