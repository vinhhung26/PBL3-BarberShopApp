using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/services")]
    [ApiController]
    public class ServicesApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;
        public ServicesApiController(BarberShopAppContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Services
                .Select(s => new {
                    s.ServiceId,
                    s.ServiceName,
                    s.Price,
                    s.DurationMinutes
                }).ToListAsync();
            return Ok(list);
        }
    }
}