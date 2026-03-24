using System;

namespace BLL.DTOs
{
    public class VendorProfileResponseDTO
    {
        public int VendorId { get; set; }
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        // Có thể bổ sung thêm Tên chủ/Ngày tạo từ User entity nếu cần
        public string OwnerName { get; set; } 
        public string? StallCode { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
