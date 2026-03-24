using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs
{
    public class StallContractExportDTO
    {
        public int ContractId { get; set; }
        public string MarketName { get; set; }
        public string MarketAddress { get; set; }
        public string StallCode { get; set; }
        public string ZoneName { get; set; }
        public decimal? AreaM2 { get; set; }
        public string BusinessName { get; set; }
        public string VendorFullName { get; set; }
        public string VendorPhone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal DepositAmount { get; set; }
        public List<FeeItemDTO> Fees { get; set; } = new List<FeeItemDTO>();
    }

    public class FeeItemDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
