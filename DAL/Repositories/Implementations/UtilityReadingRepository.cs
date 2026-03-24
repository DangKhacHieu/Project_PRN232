using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class UtilityReadingRepository : IUtilityReadingRepository
    {
        private readonly AppDbContext _context;

        public UtilityReadingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UtilityReading?> GetByIdAsync(int id)
        {
            return await _context.UtilityReadings
                .Include(u => u.Stall)
                .FirstOrDefaultAsync(u => u.ReadingId == id);
        }

        public async Task<UtilityReading?> GetByStallAndMonthAsync(int stallId, int month, int year)
        {
            return await _context.UtilityReadings
                .FirstOrDefaultAsync(u => u.StallId == stallId && u.Month == month && u.Year == year);
        }

        public async Task<UtilityReading?> GetPreviousMonthReadingAsync(int stallId, int currentMonth, int currentYear)
        {
            int prevMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            int prevYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            return await _context.UtilityReadings
                .FirstOrDefaultAsync(u => u.StallId == stallId && u.Month == prevMonth && u.Year == prevYear);
        }

        public async Task AddAsync(UtilityReading reading)
        {
            _context.UtilityReadings.Add(reading);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<UtilityReading> readings)
        {
            _context.UtilityReadings.AddRange(readings);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UtilityReading reading)
        {
            _context.UtilityReadings.Update(reading);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UtilityReading>> GetAllByStallIdAsync(int stallId)
        {
            return await _context.UtilityReadings
                .Where(u => u.StallId == stallId)
                .OrderByDescending(u => u.Year)
                .ThenByDescending(u => u.Month)
                .ToListAsync();
        }

        public async Task<IEnumerable<Stall>> GetAllStallsAsync()
        {
            return await _context.Stalls
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.StallCode)
                .ToListAsync();
        }
    }
}
