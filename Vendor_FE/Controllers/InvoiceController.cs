using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font;
using Microsoft.AspNetCore.Hosting;
using Vendor_FE.DTOs;
using System.Net.Http.Headers;

namespace Vendor_FE.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public InvoiceController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Invoices");
            var invoices = new List<InvoiceResponseDTO>();
            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                invoices = JsonSerializer.Deserialize<List<InvoiceResponseDTO>>(jsonStr, options) ?? new List<InvoiceResponseDTO>();
            }
            return View(invoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/Invoices/{id}/details");
            if (!response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var jsonStr = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var invoiceDetail = JsonSerializer.Deserialize<InvoiceDetailExportDTO>(jsonStr, options);
            
            if (invoiceDetail == null) return RedirectToAction(nameof(Index));

            return View(invoiceDetail);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/Invoices/{id}/details");
            if (!response.IsSuccessStatusCode)
                return BadRequest("Lỗi khi lấy dữ liệu từ Server.");

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var invoiceDetail = JsonSerializer.Deserialize<InvoiceDetailExportDTO>(jsonString, options);

            using (var memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);
                Document document = new Document(pdf);
                document.SetMargins(40, 40, 40, 40);

                // ===== LOAD FONT =====
                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                string boldFontPath = Path.Combine(_env.WebRootPath, "fonts", "arialbd.ttf");

                PdfFont normalFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(boldFontPath, PdfEncodings.IDENTITY_H);

                document.SetFont(normalFont);

                iText.Kernel.Colors.Color primaryBlue = new iText.Kernel.Colors.DeviceRgb(13, 110, 253);
                iText.Kernel.Colors.Color lightGray = new iText.Kernel.Colors.DeviceRgb(240, 240, 240);
                iText.Kernel.Colors.Color dangerRed = new iText.Kernel.Colors.DeviceRgb(220, 53, 69);

                // ===== HEADER =====
                document.Add(
                    new Paragraph("BAN QUẢN LÝ CHỢ SMART MARKET")
                        .SetFont(boldFont)
                        .SetFontSize(14)
                        .SetFontColor(primaryBlue)
                        .SetTextAlignment(TextAlignment.LEFT)
                );

                // Draw a simple border line
                Table lineTable = new Table(1).UseAllAvailableWidth().SetMarginTop(5).SetMarginBottom(15);
                lineTable.AddCell(new Cell().SetBorderTop(new iText.Layout.Borders.SolidBorder(lightGray, 1f)).SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER).SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER).SetBorderRight(iText.Layout.Borders.Border.NO_BORDER));
                document.Add(lineTable);

                document.Add(
                    new Paragraph("BIÊN LAI THU TIỀN ĐIỆN NƯỚC & DỊCH VỤ")
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20)
                );

                // ===== INFO TABLE =====
                Table infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 })).UseAllAvailableWidth().SetMarginBottom(20);
                
                Cell cellLeft = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                cellLeft.Add(new Paragraph($"Mã hóa đơn: HD-{invoiceDetail.InvoiceId:D6}").SetFont(boldFont));
                cellLeft.Add(new Paragraph($"Khách hàng: {invoiceDetail.BusinessName}"));
                infoTable.AddCell(cellLeft);

                Cell cellRight = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT);
                cellRight.Add(new Paragraph($"Vị trí gian hàng: {invoiceDetail.StallCode}").SetFont(boldFont));
                cellRight.Add(new Paragraph($"Kỳ thu: Tháng {invoiceDetail.Month}/{invoiceDetail.Year}"));
                infoTable.AddCell(cellRight);

                document.Add(infoTable);

                // ===== MAIN TABLE =====
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 1.5f, 2, 2.5f })).UseAllAvailableWidth();
                
                // HEADER TABLE
                table.AddHeaderCell(new Cell().SetBackgroundColor(primaryBlue).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE).Add(new Paragraph("STT").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(primaryBlue).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE).Add(new Paragraph("Nội dung phí").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(primaryBlue).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE).Add(new Paragraph("Số lượng").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(primaryBlue).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE).Add(new Paragraph("Đơn giá").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().SetBackgroundColor(primaryBlue).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE).Add(new Paragraph("Thành tiền").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                int stt = 1;
                bool isAlt = false;
                foreach (var item in invoiceDetail.Items)
                {
                    iText.Kernel.Colors.Color rowBg = isAlt ? lightGray : iText.Kernel.Colors.ColorConstants.WHITE;

                    table.AddCell(new Cell().SetBackgroundColor(rowBg).Add(new Paragraph(stt.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().SetBackgroundColor(rowBg).Add(new Paragraph(item.FeeName)));
                    table.AddCell(new Cell().SetBackgroundColor(rowBg).Add(new Paragraph(item.Quantity?.ToString("N0") ?? "-").SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().SetBackgroundColor(rowBg).Add(new Paragraph(item.UnitPrice?.ToString("N0") ?? "-").SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().SetBackgroundColor(rowBg).Add(new Paragraph(item.Amount.ToString("N0")).SetTextAlignment(TextAlignment.RIGHT)));
                    stt++;
                    isAlt = !isAlt;
                }
                document.Add(table);

                // ===== TOTAL =====
                Table totalTable = new Table(UnitValue.CreatePercentArray(new float[] { 7, 3 })).UseAllAvailableWidth().SetMarginTop(10);
                totalTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(new Paragraph("TỔNG CỘNG:").SetFont(boldFont).SetFontSize(14)));
                totalTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(new Paragraph($"{invoiceDetail.TotalAmount:N0} đ").SetFont(boldFont).SetFontSize(16).SetFontColor(dangerRed)));
                document.Add(totalTable);

                // FOOTER
                Table footerLine = new Table(1).UseAllAvailableWidth().SetMarginTop(30).SetMarginBottom(10);
                footerLine.AddCell(new Cell().SetBorderTop(new iText.Layout.Borders.SolidBorder(lightGray, 1f)).SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER).SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER).SetBorderRight(iText.Layout.Borders.Border.NO_BORDER));
                document.Add(footerLine);

                document.Add(
                    new Paragraph("Cảm ơn quý khách đã sử dụng dịch vụ của Smart Market!")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                        .SetFontSize(10)
                );

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", $"HoaDon_{id}.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PayWithMomo(int id)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/Invoices/{id}/momo-payment");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tạo link thanh toán MoMo. Vui lòng thử lại!";
                return RedirectToAction(nameof(Index));
            }

            var jsonStr = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonStr, options);

            // Tìm key không phân biệt hoa/thường (API trả "Url", không phải "url")
            var momoUrl = data?
                .FirstOrDefault(kv => kv.Key.Equals("url", StringComparison.OrdinalIgnoreCase))
                .Value;

            if (!string.IsNullOrEmpty(momoUrl))
                return Redirect(momoUrl);

            TempData["Error"] = "Không nhận được link thanh toán MoMo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> MomoReturn(
            int resultCode,
            string? extraData,
            string? orderId,
            string? message,
            long transId,
            long amount)
        {
            if (resultCode == 0 && int.TryParse(extraData, out int invoiceId))
            {
                var token = Request.Cookies["VendorAuth"];
                var client = _httpClientFactory.CreateClient("BackendAPI");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    paymentMethod = "MOMO",
                    paidAmount = (decimal)amount,
                    transactionCode = orderId,
                    momoTransactionId = transId.ToString()
                };

                var response = await client.PostAsJsonAsync($"api/Invoices/{invoiceId}/pay", payload);
                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                    ? "Thanh toán MoMo thành công! Vui lòng chờ nhân viên xác nhận."
                    : "Thanh toán thành công nhưng cập nhật trạng thái thất bại. Vui lòng liên hệ quản lý.";
            }
            else
            {
                TempData["Error"] = $"Thanh toán MoMo thất bại: {message ?? "Giao dịch bị huỷ."}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}