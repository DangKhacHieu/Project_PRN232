namespace Vendor_FE.Models
{
    public class VendorProfileViewModel
    {
        public int? VendorId { get; set; }
        public int? UserId { get; set; }
        public string? FullName { get; set; }
        public string? BusinessName { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        // Thêm các trường khác nếu backend cung cấp (Email, Phone, Address...)
    }
}