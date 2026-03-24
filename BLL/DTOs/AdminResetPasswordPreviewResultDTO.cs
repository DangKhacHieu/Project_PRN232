namespace BLL.DTOs
{
    public class AdminResetPasswordPreviewResultDTO
    {
        public int VendorId { get; set; }
        public string? BusinessName { get; set; }
        public string? ContactName { get; set; }
        public string? Email { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
