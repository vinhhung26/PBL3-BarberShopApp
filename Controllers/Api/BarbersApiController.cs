using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/barbers")]
    [ApiController]
    public class BarbersApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;

        public BarbersApiController(BarberShopAppContext db)
        {
            _db = db;
        }

        // GET /api/barbers
        // Response:
        // [
        //   { "barberId": 1, "fullName": "Nguyễn Văn A", "phone": "0901234567", "status": "AVAILABLE" },
        //   { "barberId": 2, "fullName": "Trần Văn B", "phone": "0907654321", "status": "BUSY" }
        // ]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var barbers = await _db.Barbers
                .Select(b => new
                {
                    b.BarberId,
                    b.FullName,
                    b.Phone,
                    Status = b.IsBusy == true ? "BUSY" : "AVAILABLE"
                })
                .ToListAsync();

            return Ok(barbers);
        }
    }
}