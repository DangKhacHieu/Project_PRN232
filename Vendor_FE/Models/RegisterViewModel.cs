using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vendor_FE.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Tên cửa hàng là bắt buộc")]
        [Display(Name = "Business name")]
        public string? BusinessName { get; set; }

        public string? Error { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(FullName))
                FullName = FullName.Trim();

            if (!string.IsNullOrWhiteSpace(Email))
                Email = Email.Trim();

            if (!string.IsNullOrWhiteSpace(Phone))
                Phone = Phone.Trim();

            if (!string.IsNullOrWhiteSpace(BusinessName))
                BusinessName = BusinessName.Trim();

            yield break;
        }
    }
}
