using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;

        public InvoiceService(IInvoiceRepository invoiceRepo)
        {
            _invoiceRepo = invoiceRepo;
        }

        public async Task<IEnumerable<InvoiceResponseDTO>> GetInvoicesByVendorIdAsync(int vendorId)
        {
            var invoices = await _invoiceRepo.GetInvoicesByVendorIdAsync(vendorId);

            return invoices.Select(i => new InvoiceResponseDTO
            {
                InvoiceId = i.InvoiceId,
                ContractId = i.ContractId,
                Month = i.Month,
                Year = i.Year,
                TotalAmount = i.TotalAmount ?? 0,
                Status = i.Status,
                CreatedAt = i.CreatedAt
            });
        }

        public async Task<string?> GenerateVietQRUrlAsync(int vendorId, int invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceByIdAndVendorIdAsync(invoiceId, vendorId);

            // Nếu không tìm thấy hoặc đã trả tiền rồi thì không gen QR nữa
            if (invoice == null || invoice.Status == "PAID") return null;

            string bankBin = "970436"; // Vietcombank
            string bankAccount = "0123456789";
            string accountName = "BAN QUAN LY CHO";
            string template = "compact";

            decimal amount = invoice.TotalAmount ?? 0;
            string addInfo = $"THANHTOAN_HD_{invoiceId}";

            string qrUrl = $"https://img.vietqr.io/image/{bankBin}-{bankAccount}-{template}.png?amount={amount}&addInfo={addInfo}&accountName={accountName}";

            // LƯU Ý THÊM: Do Entity của bạn có sẵn cột QrCodeData, 
            // bạn có thể lưu link này vào DB luôn nếu muốn để sau này load cho nhanh.
            /*
            if (string.IsNullOrEmpty(invoice.QrCodeData)) {
                invoice.QrCodeData = qrUrl;
                await _invoiceRepo.UpdateInvoiceAsync(invoice);
            }
            return invoice.QrCodeData;
            */

            return qrUrl;
        }
    }
}