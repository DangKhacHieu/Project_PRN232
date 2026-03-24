namespace BLL.DTOs
{
    public class CalculateInvoiceRequestDTO
    {
        public int ContractId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
