using System.ComponentModel.DataAnnotations;

namespace BarberShopApp.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập tài khoản")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		// Trường này cực kỳ quan trọng để phân biệt tab đang chọn
		public string? LoginType { get; set; }
	}
}