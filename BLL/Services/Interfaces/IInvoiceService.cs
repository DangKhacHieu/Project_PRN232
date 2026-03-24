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
        Task<IEnumerable<InvoiceResponseDTO>> GetAllInvoicesAsync();
        Task<string?> GenerateVietQRUrlAsync(int vendorId, int invoiceId);
        Task<InvoiceDetailExportDTO?> GetInvoiceDetailAsync(int vendorId, int invoiceId);
        Task<byte[]?> GenerateInvoicePdfAsync(int vendorId, int invoiceId);
        Task<InvoiceResponseDTO?> CalculateAndGenerateInvoiceAsync(CalculateInvoiceRequestDTO dto);
        Task<bool> ClearDebtAsync(int invoiceId, PaymentConfirmationDTO payment);
        Task<string?> GenerateMomoPaymentUrlAsync(int invoiceId, decimal amount, MomoPaymentConfig momoConfig);
        Task<bool> ProcessMomoWebhookAsync(MomoWebhookRequestDTO request, MomoPaymentConfig momoConfig);
        Task<IEnumerable<ActiveContractDTO>> GetActiveContractsAsync();
    }
}
