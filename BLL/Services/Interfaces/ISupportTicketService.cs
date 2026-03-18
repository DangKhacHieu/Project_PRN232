using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface ISupportTicketService
    {
        Task<IEnumerable<TicketResponseDTO>> GetTicketsByVendorIdAsync(int vendorId);
        Task<int> CreateTicketAsync(int vendorId, TicketCreateRequestDTO request);
        Task<bool> ConfirmTicketAsync(int ticketId, int vendorId);
    }
}
