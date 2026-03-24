
namespace BLL.Models
{
    public class RegisterRequest
    {
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;
        public string? BusinessName { get; set; } 
    }
}