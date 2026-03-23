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

        public async Task<byte[]?> GenerateInvoicePdfAsync(int vendorId, int invoiceId)
        {
            var detail = await GetInvoiceDetailAsync(vendorId, invoiceId);
            if (detail == null) return null;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf))
                {
                    // Header
                    document.Add(new iText.Layout.Element.Paragraph("BIEN LAI DIEN TU BAN QUAN LY CHO")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(20));
                    
                    document.Add(new iText.Layout.Element.Paragraph($"Ma hoa don: #{detail.InvoiceId}"));
                    document.Add(new iText.Layout.Element.Paragraph($"Khach hang: {detail.BusinessName}"));
                    document.Add(new iText.Layout.Element.Paragraph($"Sap: {detail.StallCode}"));
                    document.Add(new iText.Layout.Element.Paragraph($"Thang/Nam: {detail.Month}/{detail.Year}"));
                    
                    document.Add(new iText.Layout.Element.Paragraph("\nCHi TIET PHI:"));
                    
                    // Table
                    var table = new iText.Layout.Element.Table(4, true);
                    table.AddHeaderCell("Loai phi");
                    table.AddHeaderCell("So luong");
                    table.AddHeaderCell("Don gia");
                    table.AddHeaderCell("Thanh tien");
                    
                    foreach (var item in detail.Items)
                    {
                        table.AddCell(item.FeeName ?? "");
                        table.AddCell(item.Quantity?.ToString() ?? "0");
                        table.AddCell((item.UnitPrice ?? 0m).ToString("N0"));
                        table.AddCell(item.Amount.ToString("N0"));
                    }
                    
                    document.Add(table);
                    
                    document.Add(new iText.Layout.Element.Paragraph($"\nTTONG TIEN: {detail.TotalAmount:N0} VND")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                        .SetFontSize(16));
                        
                    document.Add(new iText.Layout.Element.Paragraph($"Trang thai: {detail.Status}")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                }
                
                return memoryStream.ToArray();
            }
        }
    }
}