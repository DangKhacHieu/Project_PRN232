using BLL.DTOs;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IContractService
    {
        Task<bool> CreateNewContract(StallContractDTO dto);
        Task<IEnumerable<StallDTO>> GetVacantStalls();
        Task<IEnumerable<StallContractDTO>> GetAllContracts();
        Task<IEnumerable<VendorDTO>> GetVendors();
        Task<IEnumerable<StallDTO>> GetVacantStallsByZone(int zoneId);
        Task<IEnumerable<ZoneDTO>> GetZonesByMarket(int marketId);
        Task<IEnumerable<MarketDTO>> GetAllMarkets();
        Task<StallContractExportDTO?> GetContractDetailForExport(int id);
        Task<bool> TerminateContract(int id, string reason);
        Task<IEnumerable<StallContractDTO>> GetContractHistory();
        Task<bool> RenewContract(int id, DateTime newEndDate, decimal newPrice);
    }
}
