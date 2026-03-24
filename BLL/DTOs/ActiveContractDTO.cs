namespace BLL.DTOs
{
    public class ActiveContractDTO
    {
        public int ContractId { get; set; }
        public int StallId { get; set; }
        public string StallCode { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public decimal MonthlyRent { get; set; }
    }
}
