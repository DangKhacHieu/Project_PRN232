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
    }
}
