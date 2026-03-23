using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceResponseDTO>> GetInvoicesByVendorIdAsync(int vendorId);
        Task<string?> GenerateVietQRUrlAsync(int vendorId, int invoiceId);
        Task<InvoiceDetailExportDTO?> GetInvoiceDetailAsync(int vendorId, int invoiceId);
        Task<byte[]?> GenerateInvoicePdfAsync(int vendorId, int invoiceId);
    }
}
