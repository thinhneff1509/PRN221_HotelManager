using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Project_Group5.Pages.Login
{
    public class RegisterModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public RegisterModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra Username không rỗng
            if (string.IsNullOrWhiteSpace(Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
            }

            // Kiểm tra Email không rỗng và định dạng hợp lệ
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!Email.Contains("@") || !Email.Contains("."))
            {
                ModelState.AddModelError("Email", "Invalid email address.");
            }

            // Kiểm tra Password không rỗng
            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }

            // Kiểm tra ConfirmPassword không rỗng và khớp với Password
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Confirm Password is required.");
            }
            else if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            }

            // Kiểm tra nếu có lỗi
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra xem Username hoặc Email đã tồn tại trong DB chưa
            var existingUser = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == Username || c.Email == Email);

            if (existingUser != null)
            {
                ErrorMessage = "Username or Email already exists.";
                return Page();
            }

            // Tạo khách hàng mới
            var customer = new Customer
            {
                Username = Username,
                Email = Email,
                Password = Password, // Trong thực tế, mật khẩu cần mã hóa
                RegisterDate = DateTime.Now,
                RoleId = 1 // Giả sử 2 là role mặc định cho người dùng
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }
    }
}
