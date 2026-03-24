using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StallContractsController : ControllerBase
    {
        private readonly IContractService _contractService;

        public StallContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _contractService.GetAllContracts());

        [HttpGet("vendors")]
        public async Task<IActionResult> GetVendors() => Ok(await _contractService.GetVendors());

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] StallContractDTO dto)
        {
            if (dto == null) return BadRequest();
            var result = await _contractService.CreateNewContract(dto);
            return result ? Ok(new { message = "Tạo hợp đồng thành công!" }) : BadRequest("Không thể tạo hợp đồng.");
        }

        [HttpGet("vacant-stalls")]
        public async Task<IActionResult> GetVacantStalls() => Ok(await _contractService.GetVacantStalls());

        [HttpGet("markets")]
        public async Task<IActionResult> GetMarkets() => Ok(await _contractService.GetAllMarkets());

        [HttpGet("zones/{marketId}")]
        public async Task<IActionResult> GetZones(int marketId) => Ok(await _contractService.GetZonesByMarket(marketId));

        [HttpGet("vacant-stalls/{zoneId}")]
        public async Task<IActionResult> GetVacantStalls(int zoneId) => Ok(await _contractService.GetVacantStallsByZone(zoneId));

        [HttpGet("export-pdf/{id}")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var data = await _contractService.GetContractDetailForExport(id);
            if (data == null) return NotFound();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Times New Roman"));

                    // --- HEADER ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col => {
                            col.Item().Text("BQL CHỢ " + data.MarketName?.ToUpper()).Bold();
                            col.Item().Text("Số: " + data.ContractId + "/HĐ-BQL").FontSize(10);
                        });
                        row.RelativeItem().Column(col => {
                            col.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold();
                            col.Item().AlignCenter().Text("Độc lập - Tự do - Hạnh phúc").Bold();
                            col.Item().AlignCenter().Text("----------o0o----------").FontSize(10);
                        });
                    });

                    // --- CONTENT ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().PaddingBottom(10).AlignCenter().Text("HỢP ĐỒNG THUÊ ĐIỂM KINH DOANH").FontSize(16).Bold();
                        col.Spacing(5);

                        col.Item().Text(t => {
                            t.Span("BÊN A (CHO THUÊ): ").Bold();
                            t.Span("BAN QUẢN LÝ CHỢ " + data.MarketName?.ToUpper());
                        });
                        col.Item().Text("Địa chỉ: " + (data.MarketAddress ?? "TP. Cần Thơ"));

                        col.Item().Text(t => {
                            t.Span("BÊN B (THUÊ): ").Bold();
                            t.Span(data.BusinessName?.ToUpper() ?? "N/A");
                        });
                        col.Item().Text($"Đại diện: {data.VendorFullName} - SĐT: {data.VendorPhone}");

                        col.Item().PaddingTop(10).Text("NỘI DUNG THỎA THUẬN:").Bold().Underline();
                        col.Item().Text($"- Vị trí: Sạp {data.StallCode} ({data.ZoneName})");
                        col.Item().Text($"- Diện tích: {data.AreaM2} m2");
                        col.Item().Text($"- Thời hạn: {data.StartDate:dd/MM/yyyy} đến {data.EndDate:dd/MM/yyyy}");

                        // SỬA LỖI COLOR Ở ĐÂY: Dùng FontColor() thay vì Color()
                        col.Item().Text(t => {
                            t.Span("- Giá thuê: ");
                            t.Span($"{data.MonthlyRent:N0} VNĐ/tháng").Bold().FontColor(Colors.Red.Medium);
                        });

                        col.Item().Text($"- Tiền cọc: {data.DepositAmount:N0} VNĐ");

                        if (data.Fees != null && data.Fees.Any())
                        {
                            col.Item().PaddingTop(10).Text("DANH MỤC PHÍ DỊCH VỤ:").Bold();
                            foreach (var fee in data.Fees)
                            {
                                col.Item().Text($"• {fee.Name}: {fee.Price:N0} VNĐ");
                            }
                        }
                    });

                    // --- FOOTER ---
                    page.Footer().PaddingTop(30).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(c => {
                            c.Item().Text("ĐẠI DIỆN BÊN B").Bold();
                            c.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(10);
                            c.Item().PaddingTop(35).Text(data.VendorFullName).Bold();
                        });
                        row.RelativeItem().AlignCenter().Column(c => {
                            c.Item().Text("ĐẠI DIỆN BÊN A").Bold();
                            c.Item().Text("(Ký tên, đóng dấu)").Italic().FontSize(10);
                            c.Item().PaddingTop(35).Text("TRƯỞNG BQL CHỢ").Bold();
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", $"HopDong_{data.StallCode}.pdf");
        }
        [HttpPut("terminate/{id}")]
        public async Task<IActionResult> Terminate(int id, [FromBody] string reason)
        {
            var result = await _contractService.TerminateContract(id, reason);
            return result ? Ok() : BadRequest("Không thể thanh lý hợp đồng.");
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
            => Ok(await _contractService.GetContractHistory());

        [HttpPut("renew/{id}")]
        public async Task<IActionResult> Renew(int id, [FromBody] RenewRequest request)
        {
            var result = await _contractService.RenewContract(id, request.NewEndDate, request.NewPrice);
            if (result) return Ok(new { message = "Gia hạn thành công!" });
            return BadRequest("Dữ liệu gia hạn không hợp lệ.");
        }

        // DTO nhỏ dùng riêng cho Request này
        public class RenewRequest
        {
            public DateTime NewEndDate { get; set; }
            public decimal NewPrice { get; set; }
        }
    }
}