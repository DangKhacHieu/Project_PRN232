
using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email or Phone")]
        public string Identifier { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public string? Error { get; set; }
    }
}