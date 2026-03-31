using Microsoft.AspNetCore.Mvc;

namespace BarberShopApp.Controllers
{
	public class AdminController : Controller
	{
		public IActionResult Index()
		{
			ViewBag.TotalRevenue = "15.500.000đ";
			ViewBag.TotalBills = 42;
			ViewBag.ActiveBarbers = 5;
			return View();
		}
	}
}