using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
	public class VendorService : IVendorService
	{
		private readonly IVendorRepository _vendorRepo;

		public VendorService(IVendorRepository vendorRepo)
		{
			_vendorRepo = vendorRepo;
		}

		public async Task<List<VendorDTO>> GetAllVendorsAsync()
		{
			var users = await _vendorRepo.GetAllVendorsAsync();

			return users.Select(u => new VendorDTO
			{
				VendorId = u.UserId,
				// Nếu User có VendorProfile thì lấy BusinessName, không có thì lấy FullName xài tạm
				BusinessName = u.VendorProfile?.BusinessName ?? u.FullName,
				CreatedAt = u.CreatedAt,
				User = new VendorUserDetailDTO
				{
					FullName = u.FullName,
					Email = u.Email,
					Phone = u.Phone
				}
			}).ToList();
		}

		public async Task DeleteVendorAsync(int vendorId)
		{
			var user = await _vendorRepo.GetVendorByIdAsync(vendorId);
			if (user == null) throw new Exception("Không tìm thấy tiểu thương này trong hệ thống!");

			await _vendorRepo.DeleteVendorAsync(user);
		}
	}
}
