using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;

        public CustomersApiController(BarberShopAppContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------------------
        // GET /api/customers
        // Lấy danh sách khách hàng (có thể tìm theo tên hoặc SĐT)
        // Query: ?search=0901234567
        // Response:
        // [
        //   {
        //     "customerId": 1, "fullName": "Nguyễn A", "phone": "0901234567",
        //     "customerTier": "Thành viên", "rewardPoints": 50,
        //     "dateOfBirth": "1995-05-20"
        //   }
        // ]
        // ----------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Phone.Contains(search) ||
                    (c.FullName != null && c.FullName.Contains(search)));
            }

            var list = await query
                .OrderBy(c => c.FullName)
                .Select(c => new
                {
                    c.CustomerId,
                    c.FullName,
                    c.Phone,
                    c.CustomerTier,
                    c.RewardPoints,
                    DateOfBirth = c.DateOfBirth.HasValue ? c.DateOfBirth.Value.ToString("yyyy-MM-dd") : null
                })
                .ToListAsync();

            return Ok(list);
        }

        // ----------------------------------------------------------------
        // GET /api/customers/:id
        // Chi tiết khách hàng kèm lịch sử hóa đơn
        // Response:
        // {
        //   "customerId": 1, "fullName": "Nguyễn A", "phone": "...",
        //   "customerTier": "Vàng", "rewardPoints": 200,
        //   "bills": [
        //     { "billId": 3, "createDate": "...", "totalAmount": 150000, "status": "PAID" }
        //   ]
        // }
        // ----------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _db.Customers
                .Include(c => c.Bills)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng." });

            return Ok(new
            {
                customer.CustomerId,
                customer.FullName,
                customer.Phone,
                customer.CustomerTier,
                customer.RewardPoints,
                DateOfBirth = customer.DateOfBirth.HasValue ? customer.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                Bills = customer.Bills.Select(b => new
                {
                    b.BillId,
                    b.CreateDate,
                    b.TotalAmount,
                    b.Status
                }).OrderByDescending(b => b.CreateDate)
            });
        }

        // ----------------------------------------------------------------
        // POST /api/customers
        // Tạo khách hàng mới (không cần tài khoản)
        // Request body:
        // {
        //   "fullName": "Nguyễn Văn A",
        //   "phone": "0901234567",
        //   "dateOfBirth": "1995-05-20"   // tùy chọn
        // }
        // Response 201:
        // { "customerId": 5, "fullName": "Nguyễn Văn A", "phone": "0901234567" }
        // Response 400: { "message": "Số điện thoại đã tồn tại." }
        // ----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone))
                return BadRequest(new { message = "Số điện thoại không được để trống." });

            bool phoneExists = await _db.Customers.AnyAsync(c => c.Phone == req.Phone);
            if (phoneExists)
                return BadRequest(new { message = "Số điện thoại đã tồn tại." });

            DateOnly? dob = null;
            if (!string.IsNullOrEmpty(req.DateOfBirth) && DateOnly.TryParse(req.DateOfBirth, out var parsedDob))
                dob = parsedDob;

            var customer = new Customer
            {
                FullName = req.FullName,
                Phone = req.Phone,
                DateOfBirth = dob,
                RewardPoints = 0,
                CustomerTier = "Thành viên"
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                customer.CustomerId,
                customer.FullName,
                customer.Phone,
                customer.CustomerTier,
                customer.RewardPoints
            });
        }

        // ----------------------------------------------------------------
        // PUT /api/customers/:id
        // Cập nhật thông tin khách hàng
        // Request body: (tất cả tùy chọn)
        // {
        //   "fullName": "Nguyễn B",
        //   "phone": "0909090909",
        //   "dateOfBirth": "1990-01-01"
        // }
        // ----------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest req)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng." });

            // Kiểm tra SĐT trùng với khách khác
            if (!string.IsNullOrEmpty(req.Phone))
            {
                bool phoneExists = await _db.Customers.AnyAsync(c => c.Phone == req.Phone && c.CustomerId != id);
                if (phoneExists)
                    return BadRequest(new { message = "Số điện thoại đã được dùng bởi khách hàng khác." });
                customer.Phone = req.Phone;
            }

            if (!string.IsNullOrEmpty(req.FullName))
                customer.FullName = req.FullName;

            if (!string.IsNullOrEmpty(req.DateOfBirth) && DateOnly.TryParse(req.DateOfBirth, out var dob))
                customer.DateOfBirth = dob;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công.", customer.CustomerId, customer.FullName, customer.Phone });
        }

        // ----------------------------------------------------------------
        // GET /api/customers/:id/appointments
        // Lịch hẹn của khách hàng
        // ----------------------------------------------------------------
        [HttpGet("{id}/appointments")]
        public async Task<IActionResult> GetAppointments(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng." });

            var appointments = await _db.Appointments
                .Include(a => a.Barber)
                .Where(a => a.CustomerId == id)
                .OrderByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentTime,
                    a.Status,
                    Barber = a.Barber == null ? null : new { a.Barber.BarberId, a.Barber.FullName }
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }

    public class CreateCustomerRequest
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? DateOfBirth { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? DateOfBirth { get; set; }
    }
}
