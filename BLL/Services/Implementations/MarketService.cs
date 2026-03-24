using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
	public class MarketService : IMarketService
	{
		private readonly IMarketRepository _marketRepo;

		public MarketService(IMarketRepository marketRepo)
		{
			_marketRepo = marketRepo;
		}

		public async Task<Market> GenerateAndSaveMarketAsync(GenerateMarketRequestDTO request)
		{
			// 1. Kiểm tra trùng tên chợ
			bool isExist = await _marketRepo.IsMarketNameExistAsync(request.MarketName);
			if (isExist)
			{
				throw new ArgumentException($"Tên chợ '{request.MarketName}' đã tồn tại trong hệ thống. Vui lòng đặt tên khác!");
			}

			// 2. Kiểm tra các khu truyền lên có bị trùng tên nhau không
			var zoneNames = request.Zones.Select(z => z.ZoneName.Trim()).ToList();
			if (zoneNames.Count != zoneNames.Distinct(StringComparer.OrdinalIgnoreCase).Count())
			{
				throw new ArgumentException("Trong cùng một chợ, các khu không được trùng tên với nhau!");
			}

			// TẠO MÃ VIẾT TẮT CHO CHỢ (Ví dụ: "Chợ Cần Thơ" -> "CCT")
			// Thuật toán: Cắt các từ theo khoảng trắng, lấy chữ cái đầu tiên rồi viết hoa toàn bộ
			string marketAbbreviation = string.Join("", request.MarketName
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(word => word[0])).ToUpper();

			var market = new Market
			{
				MarketName = request.MarketName,
				Address = request.Address,
				CreatedAt = DateTime.Now,
				Zones = new List<Zone>()
			};

			double currentZoneStartY = 0;

			var prefixCounters = new Dictionary<string, int>();

			foreach (var config in request.Zones)
			{
				int rows = (int)Math.Ceiling((double)config.NumberOfStalls / config.Columns);

				double zoneWidth = (config.Columns * config.StallWidth) + ((config.Columns + 1) * config.StallGap);
				double zoneHeight = (rows * config.StallHeight) + ((rows + 1) * config.StallGap);

				var zone = new Zone
				{
					ZoneName = config.ZoneName,
					CreatedAt = DateTime.Now,
					MinX = 0,
					MinY = currentZoneStartY,
					MaxX = zoneWidth,
					MaxY = currentZoneStartY + zoneHeight,
					Stalls = new List<Stall>()
				};

				if (!prefixCounters.ContainsKey(config.ZonePrefix))
					prefixCounters[config.ZonePrefix] = 1;

				for (int i = 0; i < config.NumberOfStalls; i++)
				{
					int col = i % config.Columns;
					int row = i / config.Columns;

					double stallX = zone.MinX.Value + config.StallGap + col * (config.StallWidth + config.StallGap);
					double stallY = zone.MinY.Value + config.StallGap + row * (config.StallHeight + config.StallGap);

					int currentSeq = prefixCounters[config.ZonePrefix]++;
					var stall = new Stall
					{
						// KẾT QUẢ ĐẦU RA CỰC KỲ CHUYÊN NGHIỆP: "CCT1-TP-01"
						StallCode = $"{marketAbbreviation}{{MARKET_ID}}-{config.ZonePrefix}-{currentSeq:D2}",

						AreaM2 = (decimal)(config.StallWidth * config.StallHeight),
						AllowedBusinessType = config.AllowedBusinessType,
						Status = "VACANT",
						Width = config.StallWidth,
						Height = config.StallHeight,
						PosX = stallX,
						PosY = stallY,
						CreatedAt = DateTime.Now
					};

					zone.Stalls.Add(stall);
				}

				market.Zones.Add(zone);
				currentZoneStartY += zoneHeight + request.ZoneGap;
			}

			// Gọi DAL để lưu vào Database
			return await _marketRepo.CreateFullMarketAsync(market);
		}

		public async Task UpdateStallPositionsAsync(List<UpdateStallPositionDTO> request)
		{
			var stallsToUpdate = request.Select(dto => new Stall
			{
				StallId = dto.StallId,
				PosX = dto.PosX,
				PosY = dto.PosY,
				Width = dto.Width,
				Height = dto.Height
			}).ToList();

			await _marketRepo.UpdateStallsPositionsAsync(stallsToUpdate);
		}
		public async Task<Market?> GetMarketLayoutAsync(int marketId)
		{
			return await _marketRepo.GetMarketWithDetailsAsync(marketId);
		}

		public async Task<Stall> AddSingleStallAsync(AddStallDTO request)
		{
			var stall = new Stall
			{
				ZoneId = request.ZoneId,
				StallCode = request.StallCode,
				Width = request.Width,
				Height = request.Height,
				AreaM2 = (decimal)(request.Width * request.Height),
				PosX = request.PosX, // Tọa độ do user click chọn trên màn hình
				PosY = request.PosY,
				Status = "VACANT",
				CreatedAt = DateTime.Now
			};
			return await _marketRepo.AddStallAsync(stall);
		}

		public async Task DeleteStallAsync(int stallId)
		{
			await _marketRepo.DeleteStallAsync(stallId);
		}

		// BỔ SUNG: HÀM CẬP NHẬT THÔNG TIN SẠP
		public async Task UpdateStallInfoAsync(int stallId, UpdateStallInfoDTO request)
		{
			var stall = await _marketRepo.GetStallByIdAsync(stallId);
			if (stall == null) throw new Exception("Không tìm thấy sạp này trong hệ thống!");

			stall.StallCode = request.StallCode;
			stall.Width = request.Width;
			stall.Height = request.Height;
			stall.AreaM2 = (decimal)(request.Width * request.Height);
			stall.AllowedBusinessType = request.AllowedBusinessType;
			stall.Status = request.Status;

			await _marketRepo.UpdateStallAsync(stall);
		}

		public async Task<List<Market>> GetAllMarketsAsync()
		{
			return await _marketRepo.GetAllMarketsAsync();
		}

		public async Task UpdateZonePositionsAsync(List<UpdateZonePositionDTO> request)
		{
			if (request == null || !request.Any()) return;

			// Map DTO -> Zone entities (only fields we update)
			var zonesToUpdate = request.Select(dto => new Zone
			{
				ZoneId = dto.ZoneId,
				MinX = dto.MinX,
				MinY = dto.MinY,
				MaxX = dto.MinX + dto.Width,
				MaxY = dto.MinY + dto.Height
			}).ToList();

			await _marketRepo.UpdateZonesPositionsAsync(zonesToUpdate);
		}

		public async Task<Zone> AddZoneWithStallsAsync(CreateZoneWithStallsDTO request)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (request.NumberOfStalls <= 0) throw new ArgumentException("NumberOfStalls must be > 0");

			var zone = new Zone
			{
				MarketId = request.MarketId,
				ZoneName = request.ZoneName,
				CreatedAt = DateTime.Now,
				MinX = request.MinX,
				MinY = request.MinY,
				MaxX = request.MinX + request.Width,
				MaxY = request.MinY + request.Height,
				Stalls = new List<Stall>()
			};

			var market = await _marketRepo.GetMarketWithDetailsAsync(request.MarketId);

			if (market != null && market.Zones.Any(z => z.ZoneName.Trim().Equals(request.ZoneName.Trim(), StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException($"Khu vực mang tên '{request.ZoneName}' đã tồn tại trong chợ này!");
			}

			string marketAbbreviation = market != null ? string.Join("", market.MarketName
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(word => word[0])).ToUpper() : "CH";

			var existingCodes = market?.Zones?.SelectMany(z => z.Stalls)?.Select(s => s.StallCode)?.ToHashSet() ?? new HashSet<string>();
			int startSeq = 1;
			while (true)
			{
				bool conflict = false;
				for (int i = 0; i < request.NumberOfStalls; i++)
				{
					string candidate = $"{marketAbbreviation}{request.MarketId}-{request.StallPrefix}-{(startSeq + i):D2}";
					if (existingCodes.Contains(candidate))
					{
						conflict = true;
						break;
					}
				}
				if (!conflict) break;
				startSeq++;
			}

			// simple grid layout: compute columns that fit by width (use integer columns)
			int columns = Math.Max(1, (int)Math.Floor((request.Width + request.StallGap) / (request.StallWidth + request.StallGap)));
			for (int i = 0; i < request.NumberOfStalls; i++)
			{
				int col = i % columns;
				int row = i / columns;

				double stallX = zone.MinX.Value + request.StallGap + col * (request.StallWidth + request.StallGap);
				double stallY = zone.MinY.Value + request.StallGap + row * (request.StallHeight + request.StallGap);

				var stall = new Stall
				{
					StallCode = $"{marketAbbreviation}{request.MarketId}-{request.StallPrefix}-{(startSeq + i):D2}",
					Width = request.StallWidth,
					Height = request.StallHeight,
					AreaM2 = (decimal)(request.StallWidth * request.StallHeight),
					PosX = stallX,
					PosY = stallY,
					Status = "VACANT",
					CreatedAt = DateTime.Now
				};

				zone.Stalls.Add(stall);
			}

			// call repository to persist zone + stalls
			var created = await _marketRepo.AddZoneWithStallsAsync(zone);
			return created;
		}

		public async Task DeleteZoneAsync(int zoneId, bool force = false)
		{
			await _marketRepo.DeleteZoneAsync(zoneId, force);
		}

		public async Task<Stall?> GetStallDetailsAsync(int stallId)
		{
			return await _marketRepo.GetStallByIdAsync(stallId);
		}

		public async Task<List<int>> SearchStallIdsAsync(int marketId, string? keyword, string? status)
		{
			// Gọi thẳng xuống Repository mà Hải đã viết code LINQ lúc nãy
			return await _marketRepo.SearchStallIdsAsync(marketId, keyword, status);
		}
	}
}
