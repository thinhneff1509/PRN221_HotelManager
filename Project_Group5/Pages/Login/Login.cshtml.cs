using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Project_Group5.Models; // Đảm bảo bạn đã thêm không gian tên cho các mô hình
using Microsoft.EntityFrameworkCore;

namespace Project_Group5.Pages.Login
{
	public class LoginModel : PageModel
	{
		private readonly Fall24_SE1745_PRN221_Group5Context _context;

		[BindProperty]
		public string Username { get; set; }

		[BindProperty]
		public string Password { get; set; }

		public LoginModel(Fall24_SE1745_PRN221_Group5Context context)
		{
			_context = context;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			// Kiểm tra thông tin đăng nhập
			var customer = await _context.Customers
										  .Include(c => c.Role)
										  .FirstOrDefaultAsync(c => c.Username == Username && c.Password == Password && c.Banned != true);

			if (customer == null)
			{
				// Nếu thông tin không đúng hoặc tài khoản bị khóa
				ModelState.AddModelError(string.Empty, "Invalid username or password.");
				return Page();
			}

			// Tạo Claims cho người dùng
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, customer.Username),
				new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
				new Claim(ClaimTypes.Role, customer.Role.Name) // Thêm Role của người dùng
            };

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			// Thiết lập cookie xác thực
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

			// Chuyển hướng đến trang chủ sau khi đăng nhập thành công
			return RedirectToPage("/Index");
		}

		public async Task<IActionResult> OnGetLogoutAsync()
		{
			// Đăng xuất
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToPage("/Index");
		}
	}
}
