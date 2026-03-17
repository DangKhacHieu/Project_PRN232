using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetInvoicesByVendorIdAsync(int vendorId);
        Task<Invoice?> GetInvoiceByIdAndVendorIdAsync(int invoiceId, int vendorId);
        Task UpdateInvoiceAsync(Invoice invoice);
    }
}
