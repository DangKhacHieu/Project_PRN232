namespace BLL.DTOs
{
    public class PaymentConfirmationDTO
    {
        public string PaymentMethod { get; set; } = "CASH";
        public decimal PaidAmount { get; set; }
        public string? TransactionCode { get; set; }
        // For Momo callback
        public string? MomoTransactionId { get; set; }
    }
}
