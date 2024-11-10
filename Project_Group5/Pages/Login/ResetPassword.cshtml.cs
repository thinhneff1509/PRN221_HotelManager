using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Project_Group5.Models;
using Microsoft.Extensions.Configuration;

namespace Project_Group5.Pages.Login
{
    public class ResetPasswordModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public ResetPasswordModel(Fall24_SE1745_PRN221_Group5Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public void OnGet(string token)
        {
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return Page();
            }

            // Xác thực token
            var email = ValidateToken(Token);
            if (email == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid token.");
                return Page();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            // Cập nhật mật khẩu
            customer.Password = HashPassword(Password); // Bạn cần có một hàm HashPassword để mã hóa mật khẩu
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            Message = "Your password has been reset successfully.";
            return Page();
        }

        private string ValidateToken(string token)
        {
            // Giải mã token và lấy email
            var tokenParts = Encoding.UTF8.GetString(Convert.FromBase64String(token)).Split(':');
            if (tokenParts.Length != 2)
                return null;

            var email = tokenParts[0];
            var timestamp = tokenParts[1];

            // Kiểm tra thời gian hết hạn (tuỳ chỉnh theo yêu cầu của bạn)
            var tokenTime = DateTime.Parse(timestamp);
            if (DateTime.UtcNow - tokenTime > TimeSpan.FromHours(1)) // Ví dụ: token hết hạn sau 1 giờ
                return null;

            return email;
        }

        private string HashPassword(string password)
        {
            // Hàm mã hóa mật khẩu (bạn cần thực hiện theo cách an toàn)
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("YourSecretKeyHere")))
            {
                var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedPassword);
            }
        }
    }
}