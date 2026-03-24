using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Admin_FE.Controllers
{
    public class ContractsController : Controller
    {
        private readonly HttpClient _http;

        // Sử dụng IHttpClientFactory để quản lý HttpClient chuyên nghiệp
        public ContractsController(IHttpClientFactory factory)
        {
            // "MyAPI" được cấu hình trong Program.cs với BaseAddress của API_BE
            _http = factory.CreateClient("MyAPI");
        }

        // 1. Hiển thị danh sách hợp đồng (Index)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Gọi API lấy toàn bộ danh sách hợp đồng
                var contracts = await _http.GetFromJsonAsync<List<StallContractDTO>>("api/StallContracts");
                return View(contracts);
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể kết nối với máy chủ API để lấy danh sách.";
                return View(new List<StallContractDTO>());
            }
        }

        // 2. Giao diện Tạo hợp đồng (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadDropdownData();
                return View();
            }
            catch (Exception)
            {
                TempData["Error"] = "Lỗi khi tải dữ liệu khởi tạo. Vui lòng kiểm tra API.";
                return RedirectToAction("Index");
            }
        }

        // 3. Xử lý lưu hợp đồng (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StallContractDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return View(model);
            }

            try
            {
                // Gọi đến Endpoint vừa tạo ở trên
                var response = await _http.PostAsJsonAsync("api/StallContracts/create", model);

                if (response.IsSuccessStatusCode)
                {
                    // Thông báo thành công sẽ hiển thị ở trang Index sau khi Redirect
                    TempData["Success"] = "Hợp đồng đã được ký kết thành công!";
                    return RedirectToAction("Index");
                }

                // Đọc lỗi từ API nếu có (tùy chọn)
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Lưu thất bại: " + (string.IsNullOrEmpty(errorMsg) ? "Có thể sạp này vừa được người khác thuê." : errorMsg);
            }
            catch (Exception)
            {
                TempData["Error"] = "Đã có lỗi xảy ra trong quá trình kết nối máy chủ.";
            }

            // Nếu lỗi, load lại dữ liệu cho Dropdown và hiển thị lại View Create
            await LoadDropdownData();
            return View(model);
        }

        // Hàm phụ để load lại dữ liệu khi quay về View do lỗi
        private async Task LoadDropdownData()
        {
            // Sử dụng endpoint này để đảm bảo lấy được danh sách gộp tên + SĐT
            var vendors = await _http.GetFromJsonAsync<List<VendorDTO>>("api/StallContracts/vendors");
            ViewBag.Vendors = vendors ?? new List<VendorDTO>();

            // Các dropdown khác giữ nguyên
            var markets = await _http.GetFromJsonAsync<List<MarketDTO>>("api/StallContracts/markets");
            ViewBag.Markets = markets ?? new List<MarketDTO>();
        }

        // 4. Xem lịch sử hợp đồng
        [HttpGet]
        public async Task<IActionResult> History()
        {
            try
            {
                var history = await _http.GetFromJsonAsync<List<StallContractDTO>>("api/StallContracts/history");
                return View(history);
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể tải dữ liệu lịch sử.";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public IActionResult DownloadPdf(int id)
        {
            // Đường dẫn này phải khớp với Route trong API StallContractsController
            return Redirect($"https://localhost:7169/api/StallContracts/export-pdf/{id}");
        }
        
    }
}