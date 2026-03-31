using Microsoft.AspNetCore.Mvc;

namespace BarberShopApp.Controllers
{
    public class ReceptionistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}