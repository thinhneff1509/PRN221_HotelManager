using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Project_Group5.Models;
using Microsoft.Extensions.Configuration;

namespace Project_Group5.Pages.Login
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public string Email { get; set; }

        public ForgotPasswordModel(Fall24_SE1745_PRN221_Group5Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Email))
                {
                    ModelState.AddModelError(string.Empty, "Email is required.");
                    return Page();
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == Email);

                if (customer == null)
                {
                    ModelState.AddModelError(string.Empty, "Email not found.");
                    return Page();
                }

                string token = GenerateToken(customer.Email);
                string resetLink = Url.Page("/Login/ResetPassword", null, new { token = token }, Request.Scheme);

                if (string.IsNullOrEmpty(resetLink))
                {
                    ModelState.AddModelError(string.Empty, "Reset link generation failed.");
                    return Page();
                }

                try
                {
                    SendEmail(customer.Email, "Password Reset Request",
                        $"Please reset your password by clicking <a href='{resetLink}'>here</a>.");
                    TempData["Message"] = "A reset link has been sent to your email.";
                    //return RedirectToPage("/Login/ResetPassword");
                    return Page();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send email. Please try again later.");
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                Console.WriteLine($"General error in OnPostAsync: {ex.Message}");
                return Page();
            }
        }

        private string GenerateToken(string email)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var secretKey = _configuration["EmailSettings:SecretKey"];

                // Tạo chuỗi data cần mã hóa
                var dataToHash = $"{email}|{timestamp}";

                // Tạo HMAC từ data
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                    var hashString = Convert.ToBase64String(hashBytes);

                    // Tạo token cuối cùng
                    var finalToken = $"{email}|{timestamp}|{hashString}";
                    // Encode toàn bộ token
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(finalToken));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token generation error: {ex.Message}");
                throw;
            }
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                throw new ArgumentException("Recipient email address cannot be null or empty.");
            }

            var smtpServer = _configuration["EmailSettings:SMTPServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SMTPPort"]);
            var smtpUser = _configuration["EmailSettings:SMTPUser"];
            var smtpPassword = _configuration["EmailSettings:SMTPPassword"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new InvalidOperationException("Email settings are not properly configured.");
            }

            try
            {
                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                    smtpClient.EnableSsl = true;

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(smtpUser);
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;
                        message.To.Add(toEmail);

                        smtpClient.Send(message);
                    }
                }
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
                Console.WriteLine($"StatusCode: {ex.StatusCode}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in SendEmail: {ex.Message}");
                throw;
            }
        }
    }
}