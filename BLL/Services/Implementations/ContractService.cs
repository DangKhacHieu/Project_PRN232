using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepo;

        public ContractService(IContractRepository contractRepo)
        {
            _contractRepo = contractRepo;
        }

        public async Task<IEnumerable<StallContractDTO>> GetAllContracts()
        {
            var contracts = await _contractRepo.GetAllWithDetailsAsync();

            return contracts.Select(c => new StallContractDTO
            {
                ContractId = c.ContractId,
                StallId = c.StallId,
                StallCode = c.Stall?.StallCode ?? "N/A", // Xử lý null
                VendorId = c.VendorId,
                BusinessName = c.Vendor?.BusinessName ?? "N/A", // Xử lý null
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRent = c.MonthlyRent,
                Status = c.Status ?? "ACTIVE"
            });
        }

        public async Task<IEnumerable<StallDTO>> GetVacantStalls()
        {
            var stalls = await _contractRepo.GetVacantStallsAsync();
            return stalls.Select(s => new StallDTO
            {
                StallId = s.StallId,
                StallCode = s.StallCode,
                AreaM2 = s.AreaM2
            });
        }

        public async Task<bool> CreateNewContract(StallContractDTO dto)
        {
            if (dto.EndDate <= dto.StartDate) return false;

            var entity = new StallContract
            {
                StallId = dto.StallId,
                VendorId = dto.VendorId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyRent = dto.MonthlyRent,
                DepositAmount = dto.DepositAmount,
                Status = "ACTIVE",
                CreatedAt = DateTime.Now
            };

            return await _contractRepo.CreateContractAsync(entity);
        }

        public async Task<IEnumerable<VendorDTO>> GetVendors()
        {
            var vendors = await _contractRepo.GetVendorsAsync();

            return vendors.Select(v => new VendorDTO
            {
                VendorId = v.VendorId,
                // Dùng toán tử ?. và ?? để tránh lỗi NullReferenceException (Fix CS8601/CS8602)
                BusinessName = $"{v.BusinessName ?? "Chưa đặt tên"} - SĐT: {v.User?.Phone ?? "N/A"} ({v.User?.FullName ?? "N/A"})",
                User = new VendorUserDetailDTO
                {
                    FullName = v.User?.FullName ?? "N/A",
                    Phone = v.User?.Phone ?? "N/A"
                }
            });
        }

        public async Task<IEnumerable<MarketDTO>> GetAllMarkets()
        {
            var markets = await _contractRepo.GetAllMarketsAsync();
            return markets.Select(m => new MarketDTO { MarketId = m.MarketId, MarketName = m.MarketName });
        }

        public async Task<IEnumerable<ZoneDTO>> GetZonesByMarket(int marketId)
        {
            var zones = await _contractRepo.GetZonesByMarketAsync(marketId);
            return zones.Select(z => new ZoneDTO { ZoneId = z.ZoneId, ZoneName = z.ZoneName });
        }

        public async Task<IEnumerable<StallDTO>> GetVacantStallsByZone(int zoneId)
        {
            var stalls = await _contractRepo.GetVacantStallsByZoneAsync(zoneId);
            return stalls.Select(s => new StallDTO { StallId = s.StallId, StallCode = s.StallCode, AreaM2 = s.AreaM2 });
        }

        public async Task<StallContractExportDTO?> GetContractDetailForExport(int id)
        {
            var c = await _contractRepo.GetByIdWithFullDetailsAsync(id);
            if (c == null) return null;

            var fees = await _contractRepo.GetCurrentEffectiveFeesAsync();

            return new StallContractExportDTO
            {
                ContractId = c.ContractId,
                MarketName = c.Stall?.Zone?.Market?.MarketName ?? "Chợ Trung Tâm",
                MarketAddress = c.Stall?.Zone?.Market?.Address ?? "TP. Cần Thơ",
                StallCode = c.Stall?.StallCode ?? "N/A",
                ZoneName = c.Stall?.Zone?.ZoneName ?? "N/A",
                AreaM2 = c.Stall?.AreaM2 ?? 0,
                BusinessName = c.Vendor?.BusinessName ?? "N/A",
                VendorFullName = c.Vendor?.User?.FullName ?? "N/A",
                VendorPhone = c.Vendor?.User?.Phone ?? "N/A",
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRent = c.MonthlyRent,
                DepositAmount = c.DepositAmount ?? 0,
                // Phải khởi tạo danh sách Fees (Tránh lỗi CS8601)
                Fees = fees?.Select(f => new FeeItemDTO
                {
                    Name = f.FeeType?.Name ?? "Phí dịch vụ",
                    Price = f.UnitPrice
                }).ToList() ?? new List<FeeItemDTO>()
            };
        }
        public async Task<bool> TerminateContract(int id, string reason)
    => await _contractRepo.TerminateContractAsync(id, reason);

        public async Task<IEnumerable<StallContractDTO>> GetContractHistory()
        {
            var contracts = await _contractRepo.GetExpiredAndTerminatedContractsAsync();
            return contracts.Select(c => new StallContractDTO
            {
                ContractId = c.ContractId,
                StallCode = c.Stall?.StallCode,
                BusinessName = c.Vendor?.BusinessName,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRent = c.MonthlyRent,
                Status = c.Status
            });
        }
        public async Task<bool> RenewContract(int id, DateTime newEndDate, decimal newPrice)
        {
            // 1. Lấy thông tin hợp đồng hiện tại
            var currentContract = await _contractRepo.GetByIdWithFullDetailsAsync(id);
            if (currentContract == null) return false;

            // 2. Validation: Ngày kết thúc mới phải lớn hơn ngày kết thúc cũ
            // VÀ phải lớn hơn hoặc bằng ngày hiện tại
            if (newEndDate <= currentContract.EndDate || newEndDate < DateTime.Today)
            {
                return false;
            }

            // 3. Nếu hợp lệ mới gọi xuống Repo để lưu
            return await _contractRepo.RenewContractAsync(id, newEndDate, newPrice);
        }
    }
}