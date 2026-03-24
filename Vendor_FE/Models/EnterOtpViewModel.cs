
using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class EnterOtpViewModel
    {
        [Required]
        [Display(Name = "OTP")]
        public string Otp { get; set; } = string.Empty;

        // optional: show email on page
        public string Email { get; set; } = string.Empty;
    }
}