using Microsoft.AspNetCore.Mvc;

namespace Admin_FE.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult Utilities()
        {
            return View();
        }

        public IActionResult Invoices()
        {
            return View();
        }
    }
}
