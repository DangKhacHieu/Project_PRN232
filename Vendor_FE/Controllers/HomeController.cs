using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Vendor_FE.Models;
using Vendor_FE.DTOs;

namespace Vendor_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            var token = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Add("Authorization", token);

            int totalProducts = 0;
            int unpaidInvoicesCount = 0;
            decimal totalPaidExpense = 0;
            int pendingTicketsCount = 0;

            try 
            {
                // 1. Get Products
                var prodRes = await client.GetAsync("api/Products");
                if(prodRes.IsSuccessStatusCode) {
                    var prodStr = await prodRes.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductResponseDTO>>(prodStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    totalProducts = products?.Count(p => p.IsActive) ?? 0;
                }

                // 2. Get Invoices
                var invRes = await client.GetAsync("api/Invoices");
                if(invRes.IsSuccessStatusCode) {
                    var invStr = await invRes.Content.ReadAsStringAsync();
                    var invoices = JsonSerializer.Deserialize<List<InvoiceResponseDTO>>(invStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if(invoices != null) {
                        unpaidInvoicesCount = invoices.Count(i => i.Status == "UNPAID" || i.Status == "Chưa thanh toán");
                        totalPaidExpense = invoices.Where(i => i.Status == "PAID" || i.Status == "Đã thanh toán").Sum(i => i.TotalAmount);
                    }
                }

                // 3. Get Support Tickets
                var ticketRes = await client.GetAsync("api/SupportTickets");
                if(ticketRes.IsSuccessStatusCode) {
                    var ticketStr = await ticketRes.Content.ReadAsStringAsync();
                    var tickets = JsonSerializer.Deserialize<List<SupportTicketResponseDTO>>(ticketStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if(tickets != null) {
                        pendingTicketsCount = tickets.Count(t => t.Status == "PENDING" || t.Status == "PROCESSING" || t.Status == "Chờ tiếp nhận" || t.Status == "Đang xử lý");
                    }
                }
            } catch(Exception ex) {
                _logger.LogError(ex, "Lỗi fetch thông tin Dashboard");
            }

            ViewBag.TotalProducts = totalProducts;
            ViewBag.UnpaidInvoices = unpaidInvoicesCount;
            ViewBag.TotalPaidExpense = totalPaidExpense;
            ViewBag.PendingTickets = pendingTicketsCount;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
