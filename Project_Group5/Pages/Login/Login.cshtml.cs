using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Project_Group5.Models; // Namespace của bạn chứa DbContext và mô hình
using System.Linq;
using System.Threading.Tasks;

namespace Project_Group5.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public LoginModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        // [BindProperty]
        // public bool RememberMe { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra thông tin từ cơ sở dữ liệu
                var customer = _context.Customers.FirstOrDefault(c => c.Username == Username && c.Password == Password);

                if (customer != null && customer.Banned == false) // Kiểm tra nếu không bị cấm (Banned)
                {
                    // Tạo các claims để đăng nhập
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customer.Username),
                        new Claim(ClaimTypes.Role, customer.Role.Name) // Lấy vai trò từ bảng Role
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Thiết lập thuộc tính phiên đăng nhập
                    // var authProperties = new AuthenticationProperties
                    // {
                    //     IsPersistent = RememberMe, // Đăng nhập duy trì nếu chọn RememberMe
                    //     ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    // };

                    // Đăng nhập người dùng
                    //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToPage("/Index"); // Chuyển hướng đến trang chính
                }
                else
                {
                    ErrorMessage = "Invalid username or password, or your account is banned."; // Thông báo nếu tài khoản không hợp lệ
                    return Page();
                }
            }

            return Page();
        }
    }
}
