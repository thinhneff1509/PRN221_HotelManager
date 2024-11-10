using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Project_Group5.Pages.Login
{
    public class ResetPasswordModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;
        private readonly IConfiguration _configuration;
        private object expectedEmail;
        private readonly PasswordHasher<Customer> _passwordHasher = new PasswordHasher<Customer>();

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

        private string ValidateToken(string token)
        {
            try
            {
                // Decode token từ base64
                var tokenBytes = Convert.FromBase64String(token);
                var decodedToken = Encoding.UTF8.GetString(tokenBytes);

                // Split token thành các phần
                var parts = decodedToken.Split('|');
                if (parts.Length != 3)
                {
                    Console.WriteLine("Invalid token format");
                    return null;
                }

                var email = parts[0];
                var timestamp = parts[1];
                var originalHash = parts[2];

                // Kiểm tra thời gian
                if (DateTime.TryParse(timestamp, out DateTime tokenTime))
                {
                    if (DateTime.UtcNow - tokenTime > TimeSpan.FromHours(1))
                    {
                        Console.WriteLine("Token expired");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid timestamp");
                    return null;
                }

                // Tái tạo hash để verify
                var secretKey = _configuration["EmailSettings:SecretKey"];
                var dataToHash = $"{email}|{timestamp}";

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                    var computedHash = Convert.ToBase64String(computedHashBytes);

                    // So sánh hash
                    if (computedHash != originalHash)
                    {
                        Console.WriteLine("Invalid hash");
                        return null;
                    }
                }

                // Verify email exists in database
                var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
                if (customer == null)
                {
                    Console.WriteLine("Email not found in database");
                    return null;
                }

                return email;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate model and check if the passwords match
            if (!ModelState.IsValid || Password != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Password fields cannot be empty and must match.");
                return Page(); // Return to the form to show validation errors
            }

            // Validate the token
            var email = ValidateToken(Token);
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired token.");
                return Page(); // Return to the form with error
            }

            // Find the customer in the database
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page(); // Return to the form if user is not found
            }

            // Save the new password (without hashing)
            customer.Password = Password;  // Directly store the plain text password
            _context.Customers.Update(customer);  // Ensure the customer entity is updated
            await _context.SaveChangesAsync();

            // Set the success message after the password is updated
            TempData["SuccessMessage"] = "Your password has been reset successfully.";

            return RedirectToPage("/Login/Login"); // Redirect to login page after successful password reset
        }





        //private string HashPassword(string password)
        //{
        //    var customer = new Customer(); // Just for hashing purpose
        //    return _passwordHasher.HashPassword(customer, password);
        //}
    }
}
