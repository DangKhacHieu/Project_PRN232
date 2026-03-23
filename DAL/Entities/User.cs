using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(150)]
        [Column("full_name")]
        public string FullName { get; set; } = null!;

        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Column("phone")]
        public string? Phone { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        // OData Navigation
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        public virtual VendorProfile? VendorProfile { get; set; }
    }
}
