using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
	public interface IVendorRepository
	{
		Task<List<User>> GetAllVendorsAsync();
		Task<User?> GetVendorByIdAsync(int userId);
		Task DeleteVendorAsync(User user);
	}
}
