using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BarberShopApp.Controllers
{
	public class CustomerController : Controller
	{
		private readonly BarberShopAppContext _context;

		public CustomerController(BarberShopAppContext context)
		{
			_context = context;
		}

		public IActionResult Index()
		{
	
			var customer = _context.Customers
								   .OrderByDescending(c => c.CustomerId)
								   .FirstOrDefault();

			if (customer == null)
			{
				return RedirectToAction("Login", "Account");
			}

			return View(customer);
		}
	}
}