using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models
{
    public class VendorProfileDTO
    {
        public int VendorId { get; set; }
        public int? UserId { get; set; }
        public string? BusinessName { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? FullName { get; set; }

        // Added so API returns contact info to frontend
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
