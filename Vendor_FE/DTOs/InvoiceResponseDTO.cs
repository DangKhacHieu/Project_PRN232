namespace Vendor_FE.DTOs
{
    public class InvoiceResponseDTO
    {
        public int InvoiceId { get; set; }
        public int TargetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
