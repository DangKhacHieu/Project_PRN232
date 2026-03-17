using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    [Table("contract_history")]
    public class ContractHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("contract_id")]
        public int? ContractId { get; set; }

        [MaxLength(50)]
        [Column("action_type")]
        public string? ActionType { get; set; }

        [Column("action_date")]
        public DateTime? ActionDate { get; set; } = DateTime.Now;

        [Column("note")]
        public string? Note { get; set; }

        [ForeignKey("ContractId")]
        public virtual StallContract? Contract { get; set; }
    }
}
