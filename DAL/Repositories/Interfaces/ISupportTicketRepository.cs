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
    }
}
