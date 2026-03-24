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
     [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // 1. LẤY DANH SÁCH HÓA ĐƠN
        [HttpGet]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMyInvoices()
        {
            int vendorId = GetVendorIdFromToken();
            var invoices = await _invoiceService.GetInvoicesByVendorIdAsync(vendorId);
            return Ok(invoices);
        }

        // 2. LẤY MÃ QR THANH TOÁN
        [HttpGet("{id}/generate-qr")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GeneratePaymentQR(int id)
        {
            int vendorId = GetVendorIdFromToken();
            var qrUrl = await _invoiceService.GenerateVietQRUrlAsync(vendorId, id);

            if (qrUrl == null) return BadRequest(new { Message = "Không thể tạo QR cho hóa đơn này." });

            return Ok(new { QrUrl = qrUrl });
        }


        // 3. LẤY HÓA ĐƠN THANH TOÁN
        [HttpGet("{id}/details")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var details = await _invoiceService.GetInvoiceDetailAsync(vendorId, id);
            if (details == null) return NotFound(new { Message = "Không tìm thấy dữ liệu hóa đơn." });

            return Ok(details);
        }

        [HttpGet("{id}/export-pdf")]
        [Authorize(Roles = "Vendor")]
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
            var claim = User.Claims.FirstOrDefault(c => c.Type == "vendor_id" || c.Type == "Vendor_id" || c.Type == "VendorId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        // --- ADMIN / FINANCE ENDPOINTS ---
        
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                return Ok(invoices);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("all/{id}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAdminInvoiceDetails(int id)
        {
            try
            {
                // Truyền vendorId = 0 để bypass kiểm tra quyền sở hữu Vendor
                var detail = await _invoiceService.GetInvoiceDetailAsync(0, id);
                if (detail == null) return NotFound(new { Message = "Invoice not found." });
                return Ok(detail);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("active-contracts")]
        [AllowAnonymous]
        // [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GetActiveContracts()
        {
            try
            {
                var contracts = await _invoiceService.GetActiveContractsAsync();
                return Ok(contracts);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("calculate")]
        [AllowAnonymous]
        // [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> CalculateInvoice([FromBody] BLL.DTOs.CalculateInvoiceRequestDTO dto)
        {
            try
            {
                var invoice = await _invoiceService.CalculateAndGenerateInvoiceAsync(dto);
                return Ok(invoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/pay")]
        [AllowAnonymous]
        // [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> ClearDebt(int id, [FromBody] BLL.DTOs.PaymentConfirmationDTO payment)
        {
            try
            {
                bool result = await _invoiceService.ClearDebtAsync(id, payment);
                if (result) return Ok(new { Message = "Đã gạch nợ thành công." });
                return BadRequest(new { Message = "Gạch nợ thất bại. Hóa đơn không tồn tại hoặc đã thanh toán." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}/momo-payment")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetMomoPaymentUrl(int id, [FromServices] Microsoft.Extensions.Configuration.IConfiguration config)
        {
            int vendorId = GetVendorIdFromToken();
            var invoice = await _invoiceService.GetInvoiceDetailAsync(vendorId, id);
            if (invoice == null) return BadRequest("Invoice not found.");
            if (invoice.Status == "PAID") return BadRequest("Invoice already paid.");

            var momoConfig = config.GetSection("MomoPaymentConfig").Get<BLL.DTOs.MomoPaymentConfig>();
            if (momoConfig == null) return StatusCode(500, "Momo configuration is missing.");

            var url = await _invoiceService.GenerateMomoPaymentUrlAsync(id, invoice.TotalAmount, momoConfig);
            if (url == null) return BadRequest(new { Message = "Could not generate Momo URL. Check API logs.", TotalAmount = invoice.TotalAmount });
            return Ok(new { Url = url });
        }
        [HttpPost("momo-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> MomoWebhook(
            [FromBody] BLL.DTOs.MomoWebhookRequestDTO request,
            [FromServices] Microsoft.Extensions.Configuration.IConfiguration config)
        {
            var momoConfig = config.GetSection("MomoPaymentConfig").Get<BLL.DTOs.MomoPaymentConfig>();
            if (momoConfig == null) return StatusCode(500);

            bool success = await _invoiceService.ProcessMomoWebhookAsync(request, momoConfig);
            return success ? Ok() : BadRequest();
        }
    }
}
