using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Vendor")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // 1. LẤY DANH SÁCH HÓA ĐƠN
        [HttpGet]
        public async Task<IActionResult> GetMyInvoices()
        {
            int vendorId = GetVendorIdFromToken();
            var invoices = await _invoiceService.GetInvoicesByVendorIdAsync(vendorId);
            return Ok(invoices);
        }

        // 2. LẤY MÃ QR THANH TOÁN
        [HttpGet("{id}/generate-qr")]
        public async Task<IActionResult> GeneratePaymentQR(int id)
        {
            int vendorId = GetVendorIdFromToken();
            var qrUrl = await _invoiceService.GenerateVietQRUrlAsync(vendorId, id);

            if (qrUrl == null) return BadRequest(new { Message = "Không thể tạo QR cho hóa đơn này." });

            return Ok(new { QrUrl = qrUrl });
        }

        // 3. XUẤT FILE PDF BIÊN LAI
        [HttpGet("{id}/export-pdf")]
        public async Task<IActionResult> ExportInvoicePdf(int id)
        {
            int vendorId = GetVendorIdFromToken();
            // Lấy thông tin hóa đơn (Bạn có thể viết thêm hàm GetInvoiceDetail trong Service)
            // Ở đây tôi giả lập data để render PDF

            using (var memoryStream = new MemoryStream())
            {
                // Khởi tạo Document của iText7
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Thêm nội dung vào PDF
                document.Add(new Paragraph("BIEN LAI THANH TOAN DIEN NUOC")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                document.Add(new Paragraph($"Ma Hoa Don: #{id}"));
                document.Add(new Paragraph($"Ngay xuat: {System.DateTime.Now:dd/MM/yyyy}"));
                document.Add(new Paragraph("--------------------------------------------------"));
                document.Add(new Paragraph("Trang thai: DA THANH TOAN"));
                // ... Thêm các thông tin khác ...

                document.Close();

                byte[] fileBytes = memoryStream.ToArray();
                return File(fileBytes, "application/pdf", $"BienLai_{id}.pdf");
            }
        }

        private int GetVendorIdFromToken()
        {
            //var claim = User.Claims.FirstOrDefault(c => c.Type == "VendorId");
            //return claim != null ? int.Parse(claim.Value) : 0;
            return 5;
        }
    }
}
