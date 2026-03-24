using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Admin_FE.Controllers
{
    public class TicketsController : Controller
    {
        private readonly HttpClient _http;

        // Inject IHttpClientFactory để gọi API chuyên nghiệp
        public TicketsController(IHttpClientFactory factory)
        {
            // "MyAPI" phải được cấu hình trong Program.cs của FE trỏ tới localhost:7169
            _http = factory.CreateClient("MyAPI");
        }

        // 1. Trang danh sách sự cố (Index)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Gọi API lấy toàn bộ danh sách Ticket (hàm GetList đã viết ở API)
                var tickets = await _http.GetFromJsonAsync<List<SupportTicketDTO>>("api/SupportTickets/list");

                // Trả về View cùng với dữ liệu
                return View(tickets ?? new List<SupportTicketDTO>());
            }
            catch (Exception ex)
            {
                // Nếu lỗi kết nối API, trả về danh sách rỗng và báo lỗi
                TempData["Error"] = "Không thể kết nối với máy chủ API để lấy danh sách sự cố.";
                return View(new List<SupportTicketDTO>());
            }
        }

        // 2. Action phụ để xử lý tải file PDF hoặc xuất báo cáo nếu cần (Optional)
        // Hiện tại các thao tác cập nhật trạng thái sẽ chạy bằng AJAX trực tiếp từ View tới API
    }
}