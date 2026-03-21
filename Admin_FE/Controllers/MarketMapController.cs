using Microsoft.AspNetCore.Mvc;

namespace Admin_FE.Controllers
{
	public class MarketMapController : Controller
	{
		// Link: /MarketMap/Index
		public IActionResult Index()
		{
			return View();
		}
		// CHUYÊN DÙNG ĐỂ TẠO MỚI SƠ ĐỒ CHỢ
		public IActionResult Create()
		{
			return View();
		}
		public IActionResult Edit()
		{
			return View();
		}
	}
}
