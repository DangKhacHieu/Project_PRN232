using BLL.DTOs;
using BLL.Services.Interfaces;
using ClosedXML.Excel;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
    public class UtilityReadingService : IUtilityReadingService
    {
        private readonly IUtilityReadingRepository _utilityReadingRepo;

        public UtilityReadingService(IUtilityReadingRepository utilityReadingRepo)
        {
            _utilityReadingRepo = utilityReadingRepo;
        }

        public async Task<UtilityReadingResponseDTO?> RecordUtilityAsync(UtilityReadingInputDTO dto)
        {
            // Check if already exists for this month
            var existing = await _utilityReadingRepo.GetByStallAndMonthAsync(dto.StallId, dto.Month, dto.Year);
            if (existing != null)
            {
                // Update new indexes
                existing.ElectricityNew = dto.ElectricityNew;
                existing.WaterNew = dto.WaterNew;
                existing.RecordedAt = DateTime.Now;
                await _utilityReadingRepo.UpdateAsync(existing);
                return MapToDTO(existing);
            }

            // Auto-fetch old indexes from previous month
            var prevReading = await _utilityReadingRepo.GetPreviousMonthReadingAsync(dto.StallId, dto.Month, dto.Year);
            decimal oldElectric = prevReading?.ElectricityNew ?? 0;
            decimal oldWater = prevReading?.WaterNew ?? 0;

            var newReading = new UtilityReading
            {
                StallId = dto.StallId,
                Month = dto.Month,
                Year = dto.Year,
                ElectricityOld = oldElectric,
                ElectricityNew = dto.ElectricityNew,
                WaterOld = oldWater,
                WaterNew = dto.WaterNew,
                RecordedAt = DateTime.Now
            };

            await _utilityReadingRepo.AddAsync(newReading);
            return MapToDTO(newReading);
        }

        public async Task<int> ImportExcelAsync(Stream fileStream, int month, int year)
        {
            var readingsToAdd = new List<UtilityReading>();
            var readingsToUpdate = new List<UtilityReading>();

            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

                foreach (var row in rows)
                {
                    if (row.Cell(1).IsEmpty()) continue;

                    if (int.TryParse(row.Cell(1).Value.ToString(), out int stallId))
                    {
                        decimal electricNew = row.Cell(2).TryGetValue<decimal>(out var en) ? en : 0;
                        decimal waterNew = row.Cell(3).TryGetValue<decimal>(out var wn) ? wn : 0;

                        var existing = await _utilityReadingRepo.GetByStallAndMonthAsync(stallId, month, year);

                        if (existing != null)
                        {
                            existing.ElectricityNew = electricNew;
                            existing.WaterNew = waterNew;
                            existing.RecordedAt = DateTime.Now;
                            readingsToUpdate.Add(existing);
                        }
                        else
                        {
                            var prevReading = await _utilityReadingRepo.GetPreviousMonthReadingAsync(stallId, month, year);
                            readingsToAdd.Add(new UtilityReading
                            {
                                StallId = stallId,
                                Month = month,
                                Year = year,
                                ElectricityOld = prevReading?.ElectricityNew ?? 0,
                                ElectricityNew = electricNew,
                                WaterOld = prevReading?.WaterNew ?? 0,
                                WaterNew = waterNew,
                                RecordedAt = DateTime.Now
                            });
                        }
                    }
                }
            }

            foreach (var r in readingsToUpdate)
            {
                await _utilityReadingRepo.UpdateAsync(r);
            }

            if (readingsToAdd.Count > 0)
            {
                await _utilityReadingRepo.AddRangeAsync(readingsToAdd);
            }

            return readingsToAdd.Count + readingsToUpdate.Count;
        }

        private UtilityReadingResponseDTO MapToDTO(UtilityReading entity)
        {
            return new UtilityReadingResponseDTO
            {
                ReadingId = entity.ReadingId,
                StallId = entity.StallId,
                Month = entity.Month ?? 0,
                Year = entity.Year ?? 0,
                ElectricityOld = entity.ElectricityOld,
                ElectricityNew = entity.ElectricityNew,
                WaterOld = entity.WaterOld,
                WaterNew = entity.WaterNew,
                RecordedAt = entity.RecordedAt
            };
        }

        public async Task<IEnumerable<StallLookupDTO>> GetAllStallsAsync()
        {
            var stalls = await _utilityReadingRepo.GetAllStallsAsync();
            return stalls.Select(s => new StallLookupDTO
            {
                StallId = s.StallId,
                StallCode = s.StallCode ?? "",
                Status = s.Status ?? ""
            });
        }
    }
}
