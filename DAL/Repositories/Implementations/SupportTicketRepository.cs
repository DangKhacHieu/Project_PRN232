using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class SupportTicketRepository : ISupportTicketRepository
    {
        private readonly AppDbContext _context;

        public SupportTicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupportTicket>> GetByVendorIdAsync(int vendorId)
        {
            return await _context.SupportTickets
                .Include(t => t.TicketImages) 
                .Where(t => t.VendorId == vendorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(SupportTicket ticket)
        {
            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<SupportTicket?> GetByIdAsync(int ticketId)
        {
            return await _context.SupportTickets
                .Include(t => t.TicketImages)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }
        public async Task UpdateAsync(SupportTicket ticket)
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SupportTicket>> GetAllWithDetailsAsync()
        {
            // Truy vấn: SupportTicket -> VendorProfile -> User (để lấy FullName/Phone)
            // Đồng thời lấy danh sách ảnh từ TicketImages
            return await _context.SupportTickets
                .Include(t => t.TicketImages)
                .Include(t => t.Vendor)
                    .ThenInclude(v => v.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket == null) return false;

            // Gán trạng thái mới
            ticket.Status = status;

            // Lưu thay đổi xuống SmartMarketDB
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<SupportTicket> GetByIdAdminAsync(int id)
        {
            return await _context.SupportTickets
                .Include(t => t.TicketImages) // Lấy danh sách ảnh từ bảng ticket_images
                .Include(t => t.Vendor)       // Kết nối bảng vendor_profiles
                    .ThenInclude(v => v.User) // Kết nối bảng users để lấy FullName, Phone
                .FirstOrDefaultAsync(t => t.TicketId == id);
        }

    }
}
