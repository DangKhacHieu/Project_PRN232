using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
	public class MarketRepository : IMarketRepository
	{
		private readonly AppDbContext _context;

		public MarketRepository(AppDbContext context)
		{
			_context = context;
		}
		public async Task<bool> IsMarketNameExistAsync(string marketName)
		{
			// Dùng ToLower() để tránh lách luật (VD: "Chợ Cần Thơ" sẽ bị coi là trùng với "chợ cần thơ")
			return await _context.Markets.AnyAsync(m => m.MarketName.ToLower() == marketName.ToLower());
		}
		// Lưu toàn bộ Chợ, Khu, Sạp trong 1 transaction ngầm của EF Core		
		public async Task<Market> CreateFullMarketAsync(Market market)
		{
			try
			{
				await _context.Markets.AddAsync(market);
				await _context.SaveChangesAsync();
				return market;
			}
			catch (DbUpdateException ex)
			{
				// Moi lỗi chi tiết nhất từ dưới đáy SQL Server lên
				string exactError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

				// Ném lỗi này ra ngoài Controller để trình duyệt (F12) có thể đọc được
				throw new Exception($"LỖI TỪ DATABASE SQL: {exactError}");
			}
		}

		// Cập nhật vị trí + kích thước sạp sau khi user kéo thả / resize
		// Repo updates PosX/PosY and Width/Height
		public async Task UpdateStallsPositionsAsync(List<Stall> updatedStalls)
		{
			foreach (var stall in updatedStalls)
			{
				_context.Entry(stall).Property(x => x.PosX).IsModified = true;
				_context.Entry(stall).Property(x => x.PosY).IsModified = true;
				if (stall.Width.HasValue) _context.Entry(stall).Property(x => x.Width).IsModified = true;
				if (stall.Height.HasValue) _context.Entry(stall).Property(x => x.Height).IsModified = true;
			}
			await _context.SaveChangesAsync();
		}

		// Thực thi trong MarketRepository
		public async Task<Market?> GetMarketWithDetailsAsync(int marketId)
		{
			// Dùng Include để lấy luôn cả Zones và Stalls bên trong
			return await _context.Markets
				.Include(m => m.Zones)
					.ThenInclude(z => z.Stalls)
				.FirstOrDefaultAsync(m => m.MarketId == marketId);
		}

		public async Task<Stall> AddStallAsync(Stall stall)
		{
			await _context.Stalls.AddAsync(stall);
			await _context.SaveChangesAsync();
			return stall;
		}

		public async Task DeleteStallAsync(int stallId)
		{
			var stall = await _context.Stalls.FindAsync(stallId);
			if (stall != null)
			{
				_context.Stalls.Remove(stall);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<Stall?> GetStallByIdAsync(int stallId)
		{
			// Tìm sạp có ID khớp với yêu cầu
			return await _context.Stalls.FirstOrDefaultAsync(s => s.StallId == stallId);
		}

		public async Task UpdateStallAsync(Stall stall)
		{
			// Cập nhật và lưu thay đổi xuống DB
			_context.Stalls.Update(stall);
			await _context.SaveChangesAsync();
		}

		public async Task<List<Market>> GetAllMarketsAsync()
		{
			return await _context.Markets.ToListAsync();
		}

		public async Task UpdateZonesPositionsAsync(List<Zone> updatedZones)
		{
			if (updatedZones == null || !updatedZones.Any()) return;

			foreach (var z in updatedZones)
			{
				// Tìm zone trong DB
				var existing = await _context.Zones.FindAsync(z.ZoneId);
				if (existing != null)
				{
					existing.MinX = z.MinX;
					existing.MinY = z.MinY;
					existing.MaxX = z.MaxX;
					existing.MaxY = z.MaxY;

					_context.Entry(existing).Property(x => x.MinX).IsModified = true;
					_context.Entry(existing).Property(x => x.MinY).IsModified = true;
					_context.Entry(existing).Property(x => x.MaxX).IsModified = true;
					_context.Entry(existing).Property(x => x.MaxY).IsModified = true;
				}
			}

			await _context.SaveChangesAsync();
		}

		public async Task<Zone> AddZoneWithStallsAsync(Zone zone)
		{
			// ensure MarketId exists (optional: validation outside)
			await _context.Zones.AddAsync(zone);
			await _context.SaveChangesAsync();

			// zone and stalls will have PKs populated
			return zone;
		}

		public async Task DeleteZoneAsync(int zoneId)
		{
			var zone = await _context.Zones.Include(z => z.Stalls).FirstOrDefaultAsync(z => z.ZoneId == zoneId);
			if (zone != null)
			{
				// Xóa tất cả các stalls liên quan đến zone này trước
				_context.Stalls.RemoveRange(zone.Stalls);

				_context.Zones.Remove(zone);
				await _context.SaveChangesAsync();
			}
		}
	}
}
