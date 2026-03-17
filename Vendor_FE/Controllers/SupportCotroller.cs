using Microsoft.AspNetCore.Mvc;

namespace Vendor_FE.Controllers
{
    public class SupportController : Controller
    {
        // 1. Giao diện Danh sách các sự cố đã báo cáo
        // Đường dẫn: /Support hoặc /Support/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Trả về file Views/Support/Index.cshtml
            return View();
        }

        // 2. Giao diện Form thêm mới báo cáo sự cố (có upload ảnh)
        // Đường dẫn: /Support/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Trả về file Views/Support/Create.cshtml
            return View();
        }
    }
}