using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var token = Request.Headers["Authorization"].ToString();

            var client = _httpClientFactory.CreateClient("BackendAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

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
                Document document = new Document(pdf);

                // ===== LOAD FONT =====
                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                string boldFontPath = Path.Combine(_env.WebRootPath, "fonts", "arialbd.ttf");

                PdfFont normalFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(boldFontPath, PdfEncodings.IDENTITY_H);

                document.SetFont(normalFont);

                // ===== HEADER =====
                document.Add(
                    new Paragraph("BAN QUẢN LÝ CHỢ SMART MARKET")
                        .SetFont(boldFont)
                        .SetFontSize(12)
                        .SetTextAlignment(TextAlignment.LEFT)
                );

                document.Add(
                    new Paragraph("BIÊN LAI THU TIỀN ĐIỆN NƯỚC & DỊCH VỤ")
                        .SetFont(boldFont)
                        .SetFontSize(16)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(15)
                );

                // ===== INFO =====
                document.Add(new Paragraph($"Mã hóa đơn: HD-{invoiceDetail.InvoiceId:D6}").SetMarginTop(10));
                document.Add(new Paragraph($"Khách hàng: {invoiceDetail.BusinessName}"));
                document.Add(new Paragraph($"Vị trí: {invoiceDetail.StallCode}"));
                document.Add(new Paragraph($"Kỳ thu: Tháng {invoiceDetail.Month}/{invoiceDetail.Year}"));

                // ===== TABLE =====
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 1.5f, 2, 2.5f }))
                    .UseAllAvailableWidth();
                table.SetMarginTop(15);

                // HEADER TABLE
                table.AddHeaderCell(new Cell().Add(new Paragraph("STT").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Nội dung phí").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));

                int stt = 1;
                foreach (var item in invoiceDetail.Items)
                {
                    table.AddCell(new Cell().Add(new Paragraph(stt.ToString()).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(item.FeeName)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Quantity?.ToString("N0") ?? "-").SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().Add(new Paragraph(item.UnitPrice?.ToString("N0") ?? "-").SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Amount.ToString("N0")).SetTextAlignment(TextAlignment.RIGHT)));
                    stt++;
                }

                document.Add(table);

                // ===== TOTAL =====
                document.Add(
                    new Paragraph($"TỔNG CỘNG: {invoiceDetail.TotalAmount:N0} VNĐ")
                        .SetFont(boldFont)
                        .SetFontSize(13)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetMarginTop(10)
                );

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", $"HoaDon_{id}.pdf");
            }
        }
    }
}