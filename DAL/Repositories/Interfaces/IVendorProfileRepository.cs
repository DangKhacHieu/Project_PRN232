using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IVendorProfileRepository
    {
        Task<VendorProfile?> GetByVendorIdAsync(int vendorId);
        Task UpdateAsync(VendorProfile profile);
    }
}
