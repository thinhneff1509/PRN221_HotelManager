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
        public DateTime? Dob { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }
            if (string.IsNullOrWhiteSpace(Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
            }
            if (!Dob.HasValue)
            {
                ModelState.AddModelError("Dob", "Date of Birth is required.");
            }
            if (string.IsNullOrWhiteSpace(Phone))
            {
                ModelState.AddModelError("Phone", "Phone number is required.");
            }
            if (string.IsNullOrWhiteSpace(Address))
            {
                ModelState.AddModelError("Address", "Address is required.");
            }

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

            var customer = new Customer
            {
                Name = Name,
                Dob = Dob,
                Phone = Phone,
                Address = Address,
                Username = Username,
                Email = Email,
                Password = Password, 
                RegisterDate = DateTime.Now,
                RoleId = 2 
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login/Login");

        }
    }
}
