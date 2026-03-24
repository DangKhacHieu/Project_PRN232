namespace Vendor_FE.Models
{
    public class VendorProfileResponseDTO
    {
        public int VendorId { get; set; }
        public string BusinessName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CoverImageUrl { get; set; } = null!;
        public string? StallCode { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
