using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class SupportTicketsController : ControllerBase
    {
        private readonly ISupportTicketService _ticketService;

        public SupportTicketsController(ISupportTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTickets()
        {
            int vendorId = GetVendorIdFromToken();
            var tickets = await _ticketService.GetTicketsByVendorIdAsync(vendorId);
            return Ok(tickets);
        }

        [HttpPost]
        // Bắt buộc dùng [FromForm] vì request chứa File(s)
        public async Task<IActionResult> CreateTicket([FromForm] TicketCreateRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            int ticketId = await _ticketService.CreateTicketAsync(vendorId, request);

            return Ok(new { Success = true, Message = "Đã gửi báo cáo sự cố thành công!" });
        }



        private int GetVendorIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type.Equals("VendorId", StringComparison.OrdinalIgnoreCase) || c.Type.Equals("vendor_id", StringComparison.OrdinalIgnoreCase));
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [HttpPut("{id}/process")]
        public async Task<IActionResult> ProcessTicket(int id)
        {
            var result = await _ticketService.ProcessTicketAsync(id);
            if (result) return Ok(new { Message = "Đã chuyển trạng thái sang Đang xử lý!" });
            return BadRequest("Không thể xử lý ticket này.");
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(int id)
        {
            var result = await _ticketService.ResolveTicketAsync(id);
            if (result) return Ok(new { Message = "Sự cố đã được giải quyết!" });
            return BadRequest("Không thể cập nhật trạng thái đã giải quyết.");
        }

        // 1. Lấy danh sách sự cố cho Admin
        // GET: api/Tickets/list
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var result = await _ticketService.GetAllTickets();
            return Ok(result);
        }

        // 2. Cập nhật trạng thái sự cố
        // PUT: api/Tickets/update-status/{id}
        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var ticket = await _ticketService.GetTicketById(id);
            if (ticket == null) return NotFound();

            string currentStatus = ticket.Status.ToUpper();
            string nextStatus = newStatus.ToUpper();

            // KIỂM TRA LUỒNG TRẠNG THÁI (State Machine)
            bool isValidTransition = false;
            if (currentStatus == "PENDING" && nextStatus == "PROCESSING") isValidTransition = true;
            else if (currentStatus == "PROCESSING" && nextStatus == "RESOLVED") isValidTransition = true;
            else if (currentStatus == "RESOLVED" && nextStatus == "CLOSED") isValidTransition = true;

            if (!isValidTransition)
            {
                return BadRequest($"Không thể chuyển từ {currentStatus} sang {nextStatus}. Vui lòng tuân thủ quy trình xử lý.");
            }

            var success = await _ticketService.UpdateStatus(id, nextStatus);
            if (success) return Ok(new { message = "Cập nhật thành công!" });

            return BadRequest("Lỗi cập nhật Database.");
        }

        // 3. Lấy chi tiết 1 sự cố (Nếu cần xem ảnh to hoặc nội dung dài)
        // GET: api/Tickets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _ticketService.GetTicketById(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }
    }
}
