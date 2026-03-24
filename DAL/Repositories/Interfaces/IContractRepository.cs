using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IContractRepository
    {
        Task<bool> CreateContractAsync(StallContract contract);
        Task<IEnumerable<StallContract>> GetAllWithDetailsAsync();
        Task<IEnumerable<Stall>> GetVacantStallsAsync();
        Task<IEnumerable<VendorProfile>> GetVendorsAsync();
        Task<IEnumerable<Stall>> GetVacantStallsByZoneAsync(int zoneId);
        Task<IEnumerable<Zone>> GetZonesByMarketAsync(int marketId);
        Task<IEnumerable<Market>> GetAllMarketsAsync();
        Task<List<FeeConfig>> GetCurrentEffectiveFeesAsync();
        Task<StallContract?> GetByIdWithFullDetailsAsync(int id);
        Task<bool> TerminateContractAsync(int contractId, string note);
        Task<IEnumerable<StallContract>> GetExpiredAndTerminatedContractsAsync();
        Task<bool> RenewContractAsync(int contractId, DateTime newEndDate, decimal newPrice);

    }
}
