using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;
using System;
using System.Linq;

namespace Project_Group5.Pages.Login
{
    public class RegisterModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        [BindProperty]
        public Customer Customer { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public RegisterModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra xem mật khẩu và xác nhận mật khẩu có khớp nhau hay không
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return Page();
            }

            // Kiểm tra xem tên người dùng (username) đã tồn tại chưa
            if (_context.Customers.Any(c => c.Username == Customer.Username))
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                return Page();
            }

            // Kiểm tra xem email đã được sử dụng chưa
            if (_context.Customers.Any(c => c.Email == Customer.Email))
            {
                ModelState.AddModelError(string.Empty, "Email is already in use.");
                return Page();
            }

            // Tạo tài khoản khách hàng mới
            var newCustomer = new Customer
            {
                Username = Customer.Username,
                Email = Customer.Email,
                Password = Password, // Bạn nên sử dụng hashing cho mật khẩu ở đây
                RegisterDate = DateTime.Now,
                RoleId = 2, // Gán vai trò mặc định (khách hàng)
                Banned = false
            };

            // Lưu khách hàng vào cơ sở dữ liệu
            _context.Customers.Add(newCustomer);
            _context.SaveChanges();

            // Chuyển hướng về trang đăng nhập
            return RedirectToPage("/Login");
        }
    }
}
