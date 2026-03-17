using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên quyền là bắt buộc")]
        [MaxLength(50)]
        [Column("role_name")]
        public string RoleName { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        // OData Navigation
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}