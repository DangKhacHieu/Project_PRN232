using BLL.DTOs;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
	public interface IMarketService
	{
		Task<Market> GenerateAndSaveMarketAsync(GenerateMarketRequestDTO request);
		Task UpdateStallPositionsAsync(List<UpdateStallPositionDTO> request);

		// 3 hàm mới cần bổ sung cho đúng kiểu trả về
		Task<Market?> GetMarketLayoutAsync(int marketId);
		Task<Stall> AddSingleStallAsync(AddStallDTO request);
		Task DeleteStallAsync(int stallId);
		Task UpdateStallInfoAsync(int stallId, UpdateStallInfoDTO request);
		Task<List<Market>> GetAllMarketsAsync();

		// NEW: cập nhật vị trí/kích thước khu
		Task UpdateZonePositionsAsync(List<UpdateZonePositionDTO> request);

		// NEW: Thêm zone mới kèm theo vị trí các stall
		Task<Zone> AddZoneWithStallsAsync(CreateZoneWithStallsDTO request);
		Task DeleteZoneAsync(int zoneId);
	}
}
