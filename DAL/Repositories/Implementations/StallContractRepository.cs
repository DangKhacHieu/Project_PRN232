using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class StallContractRepository : IStallContractRepository
    {
        private readonly AppDbContext _context;

        public StallContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StallContract?> GetByIdAsync(int id)
        {
            return await _context.StallContracts
                .Include(c => c.Stall)
                .Include(c => c.Vendor)
                .FirstOrDefaultAsync(c => c.ContractId == id);
        }

        public async Task<StallContract?> GetActiveContractByStallIdAsync(int stallId)
        {
            return await _context.StallContracts
                .Include(c => c.Stall)
                .Include(c => c.Vendor)
                .FirstOrDefaultAsync(c => c.StallId == stallId && c.Status == "ACTIVE");
        }

        public async Task<IEnumerable<StallContract>> GetAllActiveContractsAsync()
        {
            return await _context.StallContracts
                .Include(c => c.Stall)
                .Include(c => c.Vendor)
                // .Where(c => c.Status == "ACTIVE") // Tạm bỏ filter để Frontend test
                .ToListAsync();
        }
    }
}
