using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class FeeConfigRepository : IFeeConfigRepository
    {
        private readonly AppDbContext _context;

        public FeeConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeeConfig?> GetFeeConfigAsync(DateTime date, int feeTypeId)
        {
            return await _context.FeeConfigs
                .Include(f => f.FeeType)
                .Where(f => f.FeeTypeId == feeTypeId && f.EffectiveFrom <= date && (f.EffectiveTo == null || f.EffectiveTo >= date))
                .OrderByDescending(f => f.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FeeConfig>> GetAllActiveFeeConfigsAsync(DateTime date)
        {
            return await _context.FeeConfigs
                .Include(f => f.FeeType)
                .Where(f => f.EffectiveFrom <= date && (f.EffectiveTo == null || f.EffectiveTo >= date))
                .ToListAsync();
        }
    }
}
