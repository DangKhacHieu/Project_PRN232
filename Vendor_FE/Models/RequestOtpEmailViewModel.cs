using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class RequestOtpEmailViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Your email")]
        public string Email { get; set; } = string.Empty;
    }
}