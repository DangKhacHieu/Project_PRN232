using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StallContract>> GetAllWithDetailsAsync()
        {
            return await _context.StallContracts
                .Include(c => c.Stall)  // Join bảng Stalls
                .Include(c => c.Vendor) // Join bảng VendorProfiles
                .Where(c => c.Status != "TERMINATED")
                .ToListAsync();
        }

        // Lấy danh sách sạp đang trống
        public async Task<IEnumerable<Stall>> GetVacantStallsAsync()
        {
            return await _context.Stalls
                .Where(s => s.Status == "VACANT" && s.IsDeleted != true) // Đã sửa logic IsDeleted ở đây
                .ToListAsync();
        }

        public async Task<bool> CreateContractAsync(StallContract contract)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu hợp đồng
                _context.StallContracts.Add(contract);

                // 2. Tìm sạp tương ứng và đổi trạng thái
                var stall = await _context.Stalls.FindAsync(contract.StallId);
                if (stall != null)
                {
                    stall.Status = "RENTED";
                    _context.Stalls.Update(stall);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<IEnumerable<VendorProfile>> GetVendorsAsync()
        {
            return await _context.VendorProfiles
                .Include(v => v.User)
                .Where(v => v.IsDeleted != true)
                .ToListAsync();
        }

        // Lấy toàn bộ chợ
        public async Task<IEnumerable<Market>> GetAllMarketsAsync()
            => await _context.Markets.Where(m => m.IsDeleted != true).ToListAsync();

        // Lấy Khu vực theo ID Chợ
        public async Task<IEnumerable<Zone>> GetZonesByMarketAsync(int marketId)
            => await _context.Zones.Where(z => z.MarketId == marketId && z.IsDeleted != true).ToListAsync();

        // Lấy Sạp TRỐNG theo ID Khu vực
        public async Task<IEnumerable<Stall>> GetVacantStallsByZoneAsync(int zoneId)
            => await _context.Stalls.Where(s => s.ZoneId == zoneId && s.Status == "VACANT" && s.IsDeleted != true).ToListAsync();

        // Sửa kiểu trả về có dấu ? để báo hiệu có thể trả về null (Fix CS8603)
        public async Task<StallContract?> GetByIdWithFullDetailsAsync(int id)
        {
            return await _context.StallContracts
                .Include(c => c.Vendor).ThenInclude(v => v!.User) // Dùng ! để báo User không null sau khi include
                .Include(c => c.Stall).ThenInclude(s => s!.Zone).ThenInclude(z => z!.Market)
                .FirstOrDefaultAsync(c => c.ContractId == id);
        }

        // Lấy danh sách phí đang áp dụng tại thời điểm hiện tại
        public async Task<List<FeeConfig>> GetCurrentEffectiveFeesAsync()
        {
            var today = DateTime.Today;
            return await _context.FeeConfigs
                .Include(f => f.FeeType)
                .Where(f => f.EffectiveFrom <= today && (f.EffectiveTo == null || f.EffectiveTo >= today))
                .ToListAsync();
        }

        public async Task<bool> TerminateContractAsync(int contractId, string note)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.StallContracts.FindAsync(contractId);
                if (contract == null) return false;

                // 1. Cập nhật trạng thái hợp đồng
                contract.Status = "TERMINATED";

                // 2. Giải phóng sạp
                var stall = await _context.Stalls.FindAsync(contract.StallId);
                if (stall != null) stall.Status = "VACANT";

                // 3. Lưu vào lịch sử (Bảng contract_history trong DB của bạn)
                var history = new ContractHistory
                {
                    ContractId = contractId,
                    ActionType = "TERMINATED",
                    ActionDate = DateTime.Now,
                    Note = note
                };
                _context.ContractHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<IEnumerable<StallContract>> GetExpiredAndTerminatedContractsAsync()
        {
            return await _context.StallContracts
                .Include(c => c.Stall)
                .Include(c => c.Vendor)
                .Where(c => c.Status == "TERMINATED" || c.Status == "EXPIRED")
                .OrderByDescending(c => c.EndDate)
                .ToListAsync();
        }
        public async Task<bool> RenewContractAsync(int contractId, DateTime newEndDate, decimal newPrice)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var contract = await _context.StallContracts.FindAsync(contractId);
                if (contract == null) return false;

                // 1. Lưu thông tin cũ vào lịch sử trước khi đổi
                var history = new ContractHistory
                {
                    ContractId = contractId,
                    ActionType = "RENEWED",
                    ActionDate = DateTime.Now,
                    Note = $"Gia hạn đến {newEndDate:dd/MM/yyyy}. Giá mới: {newPrice:N0} VNĐ"
                };
                _context.ContractHistories.Add(history);

                // 2. Cập nhật thông tin mới cho hợp đồng
                contract.EndDate = newEndDate;
                contract.MonthlyRent = newPrice;
                contract.Status = "ACTIVE"; // Đảm bảo trạng thái là Active nếu trước đó nó đã Expired

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}