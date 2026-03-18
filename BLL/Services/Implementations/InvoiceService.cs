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

        public async Task<InvoiceDetailExportDTO?> GetInvoiceDetailAsync(int vendorId, int invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceDetailForExportAsync(invoiceId, vendorId);
            if (invoice == null) return null;

            return new InvoiceDetailExportDTO
            {
                InvoiceId = invoice.InvoiceId,
                // Tuỳ vào entity bạn mapping mà có thể là FullName hoặc VendorName
                BusinessName = invoice.Contract?.Vendor?.BusinessName ?? "Khách hàng",
                StallCode = invoice.Contract?.Stall?.StallCode ?? "Chưa cập nhật",
                Month = invoice.Month,
                Year = invoice.Year,
                TotalAmount = invoice.TotalAmount ?? 0,
                Status = invoice.Status,
                Items = invoice.InvoiceItems.Select(item => new InvoiceItemExportDTO
                {
                    FeeName = item.FeeConfig?.FeeType?.Name ?? "Phí dịch vụ",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount ?? 0
                }).ToList()
            };
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