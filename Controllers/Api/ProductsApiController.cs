using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/products")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;
        public ProductsApiController(BarberShopAppContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Products
                .Where(p => p.Stock > 0)
                .Select(p => new {
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Stock
                }).ToListAsync();
            return Ok(list);
        }
    }
}