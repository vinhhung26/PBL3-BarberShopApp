using Microsoft.AspNetCore.Mvc;
using BarberShopApp.Models;
using System.Linq;
using System;

namespace BarberShopApp.Controllers
{
	public class AccountController : Controller
	{
		private readonly BarberShopAppContext _db;

		public AccountController(BarberShopAppContext db)
		{
			_db = db;
		}

		// Trang hiển thị giao diện Đăng nhập
		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Tìm tài khoản khớp Username và Password
				var user = _db.Accounts.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

				if (user != null)
				{
					string type = model.LoginType?.ToLower() ?? "staff";

					// Trường hợp: Khách hàng (Role 3)
					if (user.RoleId == 3)
					{
						if (type == "customer")
							return RedirectToAction("Index", "Customer");

						ModelState.AddModelError("", "Vui lòng chọn tab Khách hàng để đăng nhập.");
					}
					// Trường hợp: Quản lý (1) hoặc Lễ tân (2)
					else if (type == "staff")
					{
						if (user.RoleId == 1) return RedirectToAction("Index", "Admin");
						if (user.RoleId == 2) return RedirectToAction("Index", "Receptionist");
					}
					else
					{
						ModelState.AddModelError("", "Loại tài khoản không khớp với tab đã chọn.");
					}
				}
				else
				{
					ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác!");
				}
			}
			return View("Login", model);
		}

		[HttpPost]
		public IActionResult Register(string Username, string FullName, string Phone, DateTime? DateOfBirth, string Password)
		{
			if (_db.Accounts.Any(a => a.Username == Username))
			{
				ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
				return View("Login");
			}

			// 1. Tạo Account
			var newAccount = new Account
			{
				Username = Username,
				Password = Password,
				FullName = FullName,
				RoleId = 3 
			};
			_db.Accounts.Add(newAccount);
			_db.SaveChanges();

			// 2. Tạo Customer liên kết
			var newCustomer = new Customer
			{
				AccountId = newAccount.AccountId,
				FullName = FullName,
				Phone = Phone,
				DateOfBirth = DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null,
				RewardPoints = 0,
				CustomerTier = "Thành viên"
			};
			_db.Customers.Add(newCustomer);
			_db.SaveChanges();

			return RedirectToAction("Login");
		}
	}
}