namespace BLL.Models
{
    public class LoginResult
    {
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public int? VendorId { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public int RoleId { get; set; } 
    }
}