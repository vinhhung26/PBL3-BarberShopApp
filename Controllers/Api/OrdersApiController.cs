using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;

        public OrdersApiController(BarberShopAppContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------------------
        // GET /api/orders
        // Lấy tất cả hóa đơn (PENDING + PAID)
        // Response:
        // [
        //   {
        //     "billId": 1, "status": "PENDING", "totalAmount": 150000,
        //     "createDate": "2025-01-01T10:00:00",
        //     "barberName": "Nguyễn Văn A",
        //     "customerName": "Khách lẻ"
        //   }
        // ]
        // ----------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _db.Bills
                .Include(b => b.Customer)
                .Include(b => b.BillServices).ThenInclude(bs => bs.Barber)
                .OrderByDescending(b => b.CreateDate)
                .ToListAsync();

            var result = bills.Select(b => new
            {
                b.BillId,
                b.Status,
                b.TotalAmount,
                b.CreateDate,
                CustomerName = b.Customer != null ? b.Customer.FullName : "Khách lẻ",
                BarberName = b.BillServices.FirstOrDefault()?.Barber?.FullName ?? "Chưa có"
            });

            return Ok(result);
        }

        // ----------------------------------------------------------------
        // GET /api/orders/:id
        // Lấy chi tiết 1 hóa đơn
        // Response:
        // {
        //   "billId": 1, "status": "PENDING", "totalAmount": 200000,
        //   "createDate": "2025-01-01T10:00:00",
        //   "customer": { "customerId": 1, "fullName": "Nguyễn A", "phone": "0901234567" },
        //   "services": [
        //     { "serviceId": 1, "serviceName": "Cắt tóc nam", "barberId": 1, "barberName": "Thợ A", "price": 100000 }
        //   ],
        //   "products": [
        //     { "productId": 2, "productName": "Wax tóc", "quantity": 1, "price": 50000 }
        //   ]
        // }
        // ----------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bill = await _db.Bills
                .Include(b => b.Customer)
                .Include(b => b.BillServices).ThenInclude(bs => bs.Service)
                .Include(b => b.BillServices).ThenInclude(bs => bs.Barber)
                .Include(b => b.BillProducts).ThenInclude(bp => bp.Product)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn." });

            var result = new
            {
                bill.BillId,
                bill.Status,
                bill.TotalAmount,
                bill.CreateDate,
                Customer = bill.Customer == null ? null : new
                {
                    bill.Customer.CustomerId,
                    bill.Customer.FullName,
                    bill.Customer.Phone
                },
                Services = bill.BillServices.Select(bs => new
                {
                    bs.ServiceId,
                    bs.Service.ServiceName,
                    bs.BarberId,
                    BarberName = bs.Barber.FullName,
                    bs.Price
                }),
                Products = bill.BillProducts.Select(bp => new
                {
                    bp.ProductId,
                    bp.Product.ProductName,
                    bp.Quantity,
                    bp.Price
                })
            };

            return Ok(result);
        }

        // ----------------------------------------------------------------
        // POST /api/orders
        // Tạo hóa đơn mới - bắt buộc chọn barber AVAILABLE
        // Request body:
        // {
        //   "barberId": 1,
        //   "customerId": null        // null = khách lẻ, hoặc truyền id khách
        // }
        // Response 201:
        // { "billId": 5, "status": "PENDING", "barberId": 1, "barberName": "Nguyễn Văn A" }
        // Response 400: { "message": "Thợ đang bận, vui lòng chọn thợ khác." }
        // ----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            var barber = await _db.Barbers.FindAsync(req.BarberId);
            if (barber == null)
                return NotFound(new { message = "Không tìm thấy thợ cắt tóc." });

            if (barber.IsBusy == true)
                return BadRequest(new { message = "Thợ đang bận, vui lòng chọn thợ khác." });

            // Đánh dấu thợ BUSY
            barber.IsBusy = true;

            // Tạo hóa đơn mới
            var bill = new Bill
            {
                CustomerId = req.CustomerId,
                Status = "PENDING",
                TotalAmount = 0,
                CreateDate = DateTime.Now
            };
            _db.Bills.Add(bill);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                bill.BillId,
                bill.Status,
                req.BarberId,
                BarberName = barber.FullName
            });
        }

        // ----------------------------------------------------------------
        // POST /api/orders/:id/items
        // Thêm dịch vụ hoặc sản phẩm vào hóa đơn PENDING
        // Request body (thêm dịch vụ):
        // { "type": "service", "serviceId": 1, "barberId": 1 }
        //
        // Request body (thêm sản phẩm):
        // { "type": "product", "productId": 2, "quantity": 1 }
        //
        // Response 200:
        // { "message": "Đã thêm vào hóa đơn.", "totalAmount": 250000 }
        // Response 400: { "message": "Hóa đơn đã thanh toán, không thể thêm." }
        // ----------------------------------------------------------------
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddItemRequest req)
        {
            var bill = await _db.Bills
                .Include(b => b.BillServices)
                .Include(b => b.BillProducts)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn." });

            if (bill.Status == "PAID")
                return BadRequest(new { message = "Hóa đơn đã thanh toán, không thể thêm." });

            if (req.Type?.ToLower() == "service")
            {
                var service = await _db.Services.FindAsync(req.ServiceId);
                if (service == null)
                    return NotFound(new { message = "Không tìm thấy dịch vụ." });

                var barber = await _db.Barbers.FindAsync(req.BarberId);
                if (barber == null)
                    return NotFound(new { message = "Không tìm thấy thợ cắt tóc." });

                // Tránh trùng (BillId + ServiceId + BarberId là composite key)
                bool exists = bill.BillServices.Any(bs => bs.ServiceId == req.ServiceId && bs.BarberId == req.BarberId);
                if (exists)
                    return BadRequest(new { message = "Dịch vụ này đã được thêm bởi thợ này rồi." });

                _db.BillServices.Add(new BillService
                {
                    BillId = id,
                    ServiceId = req.ServiceId!.Value,
                    BarberId = req.BarberId!.Value,
                    Price = service.Price
                });

                bill.TotalAmount = (bill.TotalAmount ?? 0) + service.Price;
            }
            else if (req.Type?.ToLower() == "product")
            {
                var product = await _db.Products.FindAsync(req.ProductId);
                if (product == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm." });

                int qty = req.Quantity > 0 ? req.Quantity : 1;

                // Nếu sản phẩm đã có trong hóa đơn → tăng số lượng
                var existing = await _db.BillProducts
                    .FirstOrDefaultAsync(bp => bp.BillId == id && bp.ProductId == req.ProductId);

                if (existing != null)
                {
                    existing.Quantity += qty;
                }
                else
                {
                    _db.BillProducts.Add(new BillProduct
                    {
                        BillId = id,
                        ProductId = req.ProductId!.Value,
                        Quantity = qty,
                        Price = product.Price
                    });
                }

                bill.TotalAmount = (bill.TotalAmount ?? 0) + product.Price * qty;
            }
            else
            {
                return BadRequest(new { message = "type phải là 'service' hoặc 'product'." });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã thêm vào hóa đơn.",
                totalAmount = bill.TotalAmount
            });
        }

        // ----------------------------------------------------------------
        // PUT /api/orders/:id/pay
        // Thanh toán hóa đơn:
        //   + status = PAID
        //   + barber của hóa đơn → AVAILABLE
        // Response 200:
        // { "message": "Thanh toán thành công.", "billId": 1, "totalAmount": 250000 }
        // Response 400: { "message": "Hóa đơn đã được thanh toán trước đó." }
        // ----------------------------------------------------------------
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> Pay(int id)
        {
            var bill = await _db.Bills
                .Include(b => b.BillServices)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn." });

            if (bill.Status == "PAID")
                return BadRequest(new { message = "Hóa đơn đã được thanh toán trước đó." });

            // Cập nhật hóa đơn
            bill.Status = "PAID";

            // Giải phóng tất cả thợ trong hóa đơn → AVAILABLE
            var barberIds = bill.BillServices.Select(bs => bs.BarberId).Distinct().ToList();
            var barbers = await _db.Barbers
                .Where(b => barberIds.Contains(b.BarberId))
                .ToListAsync();

            foreach (var barber in barbers)
                barber.IsBusy = false;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Thanh toán thành công.",
                bill.BillId,
                bill.TotalAmount
            });
        }
    }

    // ----------------------------------------------------------------
    // Request models
    // ----------------------------------------------------------------
    public class CreateOrderRequest
    {
        public int BarberId { get; set; }
        public int? CustomerId { get; set; }
    }

    public class AddItemRequest
    {
        public string Type { get; set; } = "";   // "service" | "product"
        public int? ServiceId { get; set; }
        public int? BarberId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}