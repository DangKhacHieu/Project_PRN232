using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
	public interface IMarketRepository
	{
		Task<Market> CreateFullMarketAsync(Market market);
		Task UpdateStallsPositionsAsync(List<Stall> updatedStalls);

		// Thêm vào IMarketRepository
		Task<Market?> GetMarketWithDetailsAsync(int marketId);
		Task<Stall> AddStallAsync(Stall stall);
		Task DeleteStallAsync(int stallId);
		Task<bool> IsMarketNameExistAsync(string marketName);

		// Đi tìm 1 sạp dựa vào ID
		Task<Stall?> GetStallByIdAsync(int stallId);

		// Lưu thông tin sạp đã sửa xuống Database
		Task UpdateStallAsync(Stall stall);

		Task<List<Market>> GetAllMarketsAsync();

		// NEW: Cập nhật vị trí/kích thước Khu (zones)
		Task UpdateZonesPositionsAsync(List<Zone> updatedZones);

		// NEW: Xóa khu (và các sạp / phụ thuộc bên trong)
		Task DeleteZoneAsync(int zoneId, bool force = false);

		// NEW: Thêm zone + stalls cùng lúc
		Task<Zone> AddZoneWithStallsAsync(Zone zone);

		Task<List<int>> SearchStallIdsAsync(int marketId, string? keyword, string? status);

	}
}
