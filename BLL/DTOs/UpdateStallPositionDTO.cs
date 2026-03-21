using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
	public class UpdateStallPositionDTO
	{
		public int StallId { get; set; }
		public double PosX { get; set; }
		public double PosY { get; set; }

		// New: chiều rộng & chiều cao (m)
		public double Width { get; set; }
		public double Height { get; set; }
	}
}
