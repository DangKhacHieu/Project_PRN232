using System;

namespace BLL.DTOs
{
    public class UtilityReadingInputDTO
    {
        public int StallId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ElectricityNew { get; set; }
        public decimal WaterNew { get; set; }
    }

    public class UtilityReadingResponseDTO
    {
        public int ReadingId { get; set; }
        public int StallId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? ElectricityOld { get; set; }
        public decimal? ElectricityNew { get; set; }
        public decimal? WaterOld { get; set; }
        public decimal? WaterNew { get; set; }
        public DateTime? RecordedAt { get; set; }
    }
}
