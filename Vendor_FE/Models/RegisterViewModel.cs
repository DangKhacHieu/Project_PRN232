
using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = null!;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Business name")]
        public string? BusinessName { get; set; }

        public string? Error { get; set; }
    }
}