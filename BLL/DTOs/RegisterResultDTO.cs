
namespace BLL.Models
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? UserId { get; set; }
        public int? VendorId { get; set; }
    }
}