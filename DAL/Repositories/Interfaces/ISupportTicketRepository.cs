using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface ISupportTicketRepository
    {
        Task<IEnumerable<SupportTicket>> GetByVendorIdAsync(int vendorId);
        Task CreateAsync(SupportTicket ticket);
        Task<SupportTicket> GetByIdAsync(int ticketId);
        Task UpdateAsync(SupportTicket ticket);

        //do them
        // Lấy danh sách kèm theo thông tin tiểu thương và ảnh
        Task<IEnumerable<SupportTicket>> GetAllWithDetailsAsync();

        // Cập nhật trạng thái xử lý
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<SupportTicket> GetByIdAdminAsync(int id);
    }
}
