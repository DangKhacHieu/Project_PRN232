using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Vendor")]
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

        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmTicket(int id)
        {
            int vendorId = GetVendorIdFromToken();
            var result = await _ticketService.ConfirmTicketAsync(id, vendorId);

            if (result) return Ok(new { Message = "Xác nhận sự cố đã được xử lý xong!" });
            return BadRequest("Không thể xác nhận ticket này hoặc bạn không có quyền.");
        }

        private int GetVendorIdFromToken()
        {
            //var claim = User.Claims.FirstOrDefault(c => c.Type == "VendorId");
            //return claim != null ? int.Parse(claim.Value) : 0;
            return 5;
        }
    }
}
