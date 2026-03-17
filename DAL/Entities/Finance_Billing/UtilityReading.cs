using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities.Infrastructure;

namespace DAL.Entities.Finance_Billing
{
    [Table("utility_readings")]
    public class UtilityReading
    {
        [Key]
        [Column("reading_id")]
        public int ReadingId { get; set; }

        [Required]
        [Column("stall_id")]
        public int StallId { get; set; }

        [Range(1, 12)]
        [Column("month")]
        public int? Month { get; set; }

        [Column("year")]
        public int? Year { get; set; }

        [Column("electricity_old")]
        public decimal? ElectricityOld { get; set; }

        [Column("electricity_new")]
        public decimal? ElectricityNew { get; set; }

        [Column("water_old")]
        public decimal? WaterOld { get; set; }

        [Column("water_new")]
        public decimal? WaterNew { get; set; }

        [Column("recorded_at")]
        public DateTime? RecordedAt { get; set; } = DateTime.Now;

        [ForeignKey("StallId")]
        public virtual Stall? Stall { get; set; }
    }
}
