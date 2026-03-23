using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Numerics;

namespace Admin_FE.Controllers
{
    public class VendorsController : Controller
    {
        // GET: /Vendors
        public IActionResult Index()
        {
            return View("~/Views/Vendors/Index.cshtml");

		}
    }
}