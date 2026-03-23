using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
	public class VendorRepository : IVendorRepository
	{
		private readonly AppDbContext _context;

		public VendorRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<List<User>> GetAllVendorsAsync()
		{
			// Lấy User kèm theo bảng VendorProfile, điều kiện RoleId = 2 và chưa xóa
			return await _context.Users
				.Include(u => u.VendorProfile)
				.Where(u => u.RoleId == 2 && u.IsDeleted == false)
				.OrderByDescending(u => u.CreatedAt)
				.ToListAsync();
		}

		public async Task<User?> GetVendorByIdAsync(int userId)
		{
			return await _context.Users
				.Include(u => u.VendorProfile)
				.FirstOrDefaultAsync(u => u.UserId == userId && u.RoleId == 2 && u.IsDeleted == false);
		}

		public async Task DeleteVendorAsync(User user)
		{
			// Thay vì xóa hẳn khỏi DB (Remove), mình update cờ IsDeleted = true (Xóa mềm)
			user.IsDeleted = true;
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}
	}
}
