using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IUtilityReadingRepository
    {
        Task<UtilityReading?> GetByIdAsync(int id);
        Task<UtilityReading?> GetByStallAndMonthAsync(int stallId, int month, int year);
        Task<UtilityReading?> GetPreviousMonthReadingAsync(int stallId, int currentMonth, int currentYear);
        Task AddAsync(UtilityReading reading);
        Task AddRangeAsync(IEnumerable<UtilityReading> readings);
        Task UpdateAsync(UtilityReading reading);
        Task<IEnumerable<UtilityReading>> GetAllByStallIdAsync(int stallId);
        Task<IEnumerable<Stall>> GetAllStallsAsync();
    }
}
