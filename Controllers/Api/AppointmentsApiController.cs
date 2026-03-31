using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Controllers.Api
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly BarberShopAppContext _db;

        public AppointmentsApiController(BarberShopAppContext db)
        {
            _db = db;
        }

        // ----------------------------------------------------------------
        // GET /api/appointments
        // Lấy danh sách lịch hẹn (có thể lọc theo ngày)
        // Query: ?date=2025-01-15   (tùy chọn)
        // Response:
        // [
        //   {
        //     "appointmentId": 1,
        //     "appointmentTime": "2025-01-15T09:00:00",
        //     "status": "Chờ xử lý",
        //     "customer": { "customerId": 1, "fullName": "Nguyễn A", "phone": "0901234567" },
        //     "barber": { "barberId": 1, "fullName": "Thợ A" }
        //   }
        // ]
        // ----------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? date)
        {
            var query = _db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Barber)
                .AsQueryable();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var filterDate))
            {
                query = query.Where(a => a.AppointmentTime.Date == filterDate.Date);
            }

            var list = await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentTime,
                    a.Status,
                    Customer = a.Customer == null ? null : new
                    {
                        a.Customer.CustomerId,
                        a.Customer.FullName,
                        a.Customer.Phone
                    },
                    Barber = a.Barber == null ? null : new
                    {
                        a.Barber.BarberId,
                        a.Barber.FullName
                    }
                })
                .ToListAsync();

            return Ok(list);
        }

        // ----------------------------------------------------------------
        // GET /api/appointments/:id
        // ----------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var a = await _db.Appointments
                .Include(x => x.Customer)
                .Include(x => x.Barber)
                .FirstOrDefaultAsync(x => x.AppointmentId == id);

            if (a == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            return Ok(new
            {
                a.AppointmentId,
                a.AppointmentTime,
                a.Status,
                Customer = a.Customer == null ? null : new
                {
                    a.Customer.CustomerId,
                    a.Customer.FullName,
                    a.Customer.Phone
                },
                Barber = a.Barber == null ? null : new
                {
                    a.Barber.BarberId,
                    a.Barber.FullName
                }
            });
        }

        // ----------------------------------------------------------------
        // POST /api/appointments
        // Tạo lịch hẹn mới
        // Request body:
        // {
        //   "customerId": 1,
        //   "barberId": 2,
        //   "appointmentTime": "2025-01-15T09:00:00"
        // }
        // Response 201:
        // { "appointmentId": 5, "appointmentTime": "...", "status": "Chờ xử lý" }
        // Response 400: { "message": "Thợ đã có lịch hẹn vào khung giờ này." }
        // ----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest req)
        {
            // Kiểm tra khách hàng tồn tại
            var customer = await _db.Customers.FindAsync(req.CustomerId);
            if (customer == null)
                return NotFound(new { message = "Không tìm thấy khách hàng." });

            // Kiểm tra thợ tồn tại
            var barber = await _db.Barbers.FindAsync(req.BarberId);
            if (barber == null)
                return NotFound(new { message = "Không tìm thấy thợ cắt tóc." });

            // Kiểm tra thời gian hợp lệ
            if (req.AppointmentTime <= DateTime.Now)
                return BadRequest(new { message = "Thời gian đặt lịch phải là tương lai." });

            // Kiểm tra thợ đã có lịch trong vòng 1 giờ chưa (tránh trùng lịch)
            var conflictWindow = req.AppointmentTime.AddMinutes(-59);
            bool conflict = await _db.Appointments.AnyAsync(a =>
                a.BarberId == req.BarberId &&
                a.Status != "Đã huỷ" &&
                a.AppointmentTime >= conflictWindow &&
                a.AppointmentTime <= req.AppointmentTime.AddMinutes(59));

            if (conflict)
                return BadRequest(new { message = "Thợ đã có lịch hẹn vào khung giờ này (±1 giờ)." });

            var appointment = new Appointment
            {
                CustomerId = req.CustomerId,
                BarberId = req.BarberId,
                AppointmentTime = req.AppointmentTime,
                Status = "Chờ xử lý"
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                appointment.AppointmentId,
                appointment.AppointmentTime,
                appointment.Status,
                BarberName = barber.FullName,
                CustomerName = customer.FullName
            });
        }

        // ----------------------------------------------------------------
        // PUT /api/appointments/:id/status
        // Cập nhật trạng thái lịch hẹn
        // Request body:
        // { "status": "Đã xác nhận" }   // hoặc "Đã huỷ", "Hoàn thành"
        // ----------------------------------------------------------------
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            var allowed = new[] { "Chờ xử lý", "Đã xác nhận", "Hoàn thành", "Đã huỷ" };
            if (!allowed.Contains(req.Status))
                return BadRequest(new { message = $"Trạng thái không hợp lệ. Các giá trị cho phép: {string.Join(", ", allowed)}" });

            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            appointment.Status = req.Status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công.", appointment.AppointmentId, appointment.Status });
        }

        // ----------------------------------------------------------------
        // DELETE /api/appointments/:id
        // Huỷ lịch hẹn (set status = "Đã huỷ", không xoá khỏi DB)
        // ----------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { message = "Không tìm thấy lịch hẹn." });

            if (appointment.Status == "Hoàn thành")
                return BadRequest(new { message = "Không thể huỷ lịch hẹn đã hoàn thành." });

            appointment.Status = "Đã huỷ";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đã huỷ lịch hẹn.", appointment.AppointmentId });
        }
    }

    public class CreateAppointmentRequest
    {
        public int CustomerId { get; set; }
        public int BarberId { get; set; }
        public DateTime AppointmentTime { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = "";
    }
}