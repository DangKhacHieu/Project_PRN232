using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IStallContractRepository
    {
        Task<StallContract?> GetByIdAsync(int id);
        Task<StallContract?> GetActiveContractByStallIdAsync(int stallId);
        Task<IEnumerable<StallContract>> GetAllActiveContractsAsync();
    }
}
