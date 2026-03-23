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


        // 3. LẤY HÓA ĐƠN THANH TOÁN
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var details = await _invoiceService.GetInvoiceDetailAsync(vendorId, id);
            if (details == null) return NotFound(new { Message = "Không tìm thấy dữ liệu hóa đơn." });

            return Ok(details);
        }

        [HttpGet("{id}/export-pdf")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(vendorId, id);
            if (pdfBytes == null) return NotFound(new { Message = "Không tìm thấy dữ liệu hóa đơn." });

            return File(pdfBytes, "application/pdf", $"Invoice_{id}.pdf");
        }

        private int GetVendorIdFromToken()
        {
            //var claim = User.Claims.FirstOrDefault(c => c.Type == "VendorId");
            //return claim != null ? int.Parse(claim.Value) : 0;
            return 4;
        }
    }
}
