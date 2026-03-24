using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class VendorProfileRepository : IVendorProfileRepository
    {
        private readonly AppDbContext _context;

        public VendorProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VendorProfile?> GetByVendorIdAsync(int vendorId)
        {
            return await _context.VendorProfiles
                .Include(v => v.StallContracts)
                    .ThenInclude(sc => sc.Stall)
                .FirstOrDefaultAsync(v => v.VendorId == vendorId);
        }

        public async Task UpdateAsync(VendorProfile profile)
        {
            _context.VendorProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
