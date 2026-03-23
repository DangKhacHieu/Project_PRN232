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
	public class MarketRepository : IMarketRepository
	{
		private readonly AppDbContext _context;

		public MarketRepository(AppDbContext context)
		{
			_context = context;
		}
		public async Task<bool> IsMarketNameExistAsync(string marketName)
		{
			return await _context.Markets.AnyAsync(m => m.MarketName.ToLower() == marketName.ToLower());
		}

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
				string exactError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				throw new Exception($"LỖI TỪ DATABASE SQL: {exactError}");
			}
		}

		// Cập nhật vị trí + kích thước sạp sau khi user kéo thả / resize
		public async Task UpdateStallsPositionsAsync(List<Stall> updatedStalls)
		{
			if (updatedStalls == null || !updatedStalls.Any()) return;

			foreach (var stall in updatedStalls)
			{
				// Find existing entity to avoid EF tracking issues and to skip soft-deleted ones
				var existing = await _context.Stalls.FindAsync(stall.StallId);
				if (existing == null) continue;
				if (existing.IsDeleted) continue; // skip deleted stalls

				// Update only changed fields
				existing.PosX = stall.PosX;
				existing.PosY = stall.PosY;

				if (stall.Width.HasValue)
					existing.Width = stall.Width;
				if (stall.Height.HasValue)
					existing.Height = stall.Height;

				_context.Stalls.Update(existing);
			}

			await _context.SaveChangesAsync();
		}

		// Thực thi trong MarketRepository
		public async Task<Market?> GetMarketWithDetailsAsync(int marketId)
		{
			// Lấy Market + Zones + Stalls
			var market = await _context.Markets
				.Include(m => m.Zones)
					.ThenInclude(z => z.Stalls)
				.FirstOrDefaultAsync(m => m.MarketId == marketId);

			if (market == null) return null;

			// Loại bỏ các sạp bị soft-deleted trước khi trả cho FE
			foreach (var zone in market.Zones)	
			{
				zone.Stalls = zone.Stalls.Where(s => !s.IsDeleted).ToList();
			}

			// Filter out soft-deleted zones/stalls defensively:
			market.Zones = market.Zones
				.Where(z => !(z.IsDeleted))
				.Select(z => {
					z.Stalls = z.Stalls.Where(s => !(s.IsDeleted)).ToList();
					return z;
				}).ToList();

			return market;
		}

		public async Task<Stall> AddStallAsync(Stall stall)
		{
			await _context.Stalls.AddAsync(stall);
			await _context.SaveChangesAsync();
			return stall;
		}

		// Soft-delete: set IsDeleted = true
		public async Task DeleteStallAsync(int stallId)
		{
			var stall = await _context.Stalls.FindAsync(	stallId);
			if (stall != null)
			{
				// Only mark as deleted; do not change Status to a value that violates DB CHECK constraint
				stall.IsDeleted = true;
				_context.Stalls.Update(stall);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<Stall?> GetStallByIdAsync(int stallId)
		{
			// Tìm sạp có ID khớp; vẫn trả cả soft-deleted (caller can decide)
			return await _context.Stalls
				.Include(s => s.StallContracts)
					.ThenInclude(c => c.Vendor)
				.FirstOrDefaultAsync(s => s.StallId == stallId);
		}

		public async Task UpdateStallAsync(Stall stall)
		{
			// safer pattern: find existing and assign
			var existing = await _context.Stalls.FindAsync(stall.StallId);
			if (existing == null) throw new Exception("Stall not found");

			// if the stall is soft-deleted, optionally throw
			if (existing.IsDeleted) throw new Exception("Cannot update a deleted stall");

			existing.StallCode = stall.StallCode;
			existing.Width = stall.Width;
			existing.Height = stall.Height;
			existing.AreaM2 = stall.AreaM2;
			existing.AllowedBusinessType = stall.AllowedBusinessType;
			existing.Status = stall.Status;
			existing.PosX = stall.PosX;
			existing.PosY = stall.PosY;

			_context.Stalls.Update(existing);
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
			await _context.Zones.AddAsync(zone);
			await _context.SaveChangesAsync();
			return zone;
		}

		// safe delete zone implementation (if present)
		public async Task DeleteZoneAsync(int zoneId, bool force = false)
		{
			var zone = await _context.Zones
				.Include(z => z.Stalls)
					.ThenInclude(s => s.StallContracts)
				.FirstOrDefaultAsync(z => z.ZoneId == zoneId);

			if (zone == null) throw new KeyNotFoundException($"Zone {zoneId} not found");

			// Tìm hợp đồng ACTIVE trên các sạp
			var activeContract = zone.Stalls
				.SelectMany(s => s.StallContracts ?? new List<StallContract>())
				.FirstOrDefault(c => (c.Status ?? "").ToUpper() == "ACTIVE");

			if (activeContract != null && !force)
			{
				throw new InvalidOperationException("Khu này có sạp đang có hợp đồng ACTIVE. Nếu bạn chắc chắn muốn xóa, chọn xóa cưỡng chế (force).");
			}

			// Nếu force==true: xóa tất cả hợp đồng liên quan
			var contracts = zone.Stalls.SelectMany(s => s.StallContracts ?? new List<StallContract>()).ToList();
			if (contracts.Any())
			{
				_context.RemoveRange(contracts);
			}

			// Soft-delete stalls (giữ lịch sử) — hoặc remove tuỳ nghiệp vụ
			foreach (var s in zone.Stalls)
			{
				s.IsDeleted = true;
				_context.Stalls.Update(s);
			}

			// Remove zone (hoặc soft-delete zone nếu muốn)
			_context.Zones.Remove(zone);
			await _context.SaveChangesAsync();
		}
	}
}
