using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Vendor_FE.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Inject IHttpClientFactory vào Controller
        public InvoiceController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");

            // LƯU Ý BẢO MẬT: Nếu API yêu cầu [Authorize], bạn phải đính kèm JWT Token vào Header của client
            // var token = HttpContext.Session.GetString("JWToken"); 
            // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 2. Gọi API GET sang API_BE
            var response = await client.GetAsync("api/Invoices");

            if (response.IsSuccessStatusCode)
            {
                // 3. Đọc dữ liệu JSON trả về
                var jsonString = await response.Content.ReadAsStringAsync();

                // 4. Ép kiểu JSON sang List Object (Bạn có thể tạo ViewModels ở FE để hứng dữ liệu)
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var invoices = JsonSerializer.Deserialize<List<object>>(jsonString, options); // Thay "object" bằng ViewModel của bạn

                // 5. Trả dữ liệu ra View
                return View(invoices);
            }

            return View(new List<object>());
        }
    }
}