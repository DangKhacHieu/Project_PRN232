using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vendor_FE.Models;

namespace Vendor_FE.Controllers
{
    public class SupportController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SupportController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            AddToken(client);
            var response = await client.GetAsync("api/SupportTickets");
            var tickets = new List<SupportTicketResponseDTO>();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                tickets = JsonSerializer.Deserialize<List<SupportTicketResponseDTO>>(content, options) ?? new List<SupportTicketResponseDTO>();
            }

            return View(tickets);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string Title, string Description, List<IFormFile> Images)
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            AddToken(client);
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(Title ?? ""), "Title");
            content.Add(new StringContent(Description ?? ""), "Description");

            if (Images != null && Images.Count > 0)
            {
                foreach (var file in Images)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.Add("Content-Type", file.ContentType);
                    content.Add(streamContent, "Images", file.FileName);
                }
            }

            var response = await client.PostAsync("api/SupportTickets", content);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Gửi báo cáo sự cố thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi báo cáo!";
            return RedirectToAction(nameof(Index)); // Or return View with error
        }



        private void AddToken(HttpClient client)
        {
            var token = Request.Cookies["VendorAuth"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
