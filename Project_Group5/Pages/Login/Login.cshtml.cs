﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Threading.Tasks;
using Project_Group5.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        public IActionResult OnGet()
        {
            // Kiểm tra nếu người dùng đã đăng nhập và không phải là Customer
            if (User.Identity.IsAuthenticated)
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != "Customer") // Role khác Customer
                {
                    return RedirectToPage("/Homepage/Home"); // Điều hướng đến trang chính hoặc trang phù hợp
                }
            }
            return Page(); // Nếu chưa đăng nhập hoặc là Customer, hiển thị trang đăng nhập
        }

       
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var customer = await _context.Customers
                                          .Include(c => c.Role)
                                          .FirstOrDefaultAsync(c => c.Username == Username && c.Password == Password && c.Banned != true);

            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Create Claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.Username),
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                 new Claim(ClaimTypes.Role, customer.Role.Id == 1 ? "Admin" :
                           customer.Role.Id == 3 ? "Receptionist" : "Customer") // Set Role based on Role ID
			};

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Set authentication cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));


            // Điều hướng dựa trên `ReturnUrl` hoặc vai trò
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirect based on role
            if (customer.Role.Id == 1)
            {
                return RedirectToPage("/Admin/DashBoard/Dashboard");
            }
            else if (customer.Role.Id == 3)
            {
                return RedirectToPage("/Receptionist/Home");
            }
            else
            {
                return RedirectToPage("/Homepage/Home");
            }
        }

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Homepage/Home");
        }

        public async Task Login()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });
            return RedirectToAction("Home", "Index");
        }

        public IActionResult OnGetLogin()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("/Homepage/Home", "GoogleResponse") // Điều hướng sau khi xác thực Google thành công
            };

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }


        public async Task<IActionResult> OnGetGoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToPage("/Login");

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (email == null)
                return RedirectToPage("/Login");

            // Kiểm tra email trong database
            var customer = await _context.Customers
                                          .Include(c => c.Role)
                                          .FirstOrDefaultAsync(c => c.Email == email && c.Banned != true);

            if (customer == null)
            {
                customer = new Customer
                {
                    Username = name ?? email.Split('@')[0],
                    Email = email,
                    RoleId = 2 // Customer role
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // Tạo các claims cho người dùng với role mặc định là Customer nếu không có role khác
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, customer.Username),
        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
        new Claim(ClaimTypes.Email, customer.Email),
        new Claim(ClaimTypes.Role, customer.Role.Id == 1 ? "Admin" :
                              customer.Role.Id == 3 ? "Receptionist" : "Customer")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            return RedirectToPage("/Homepage/Home"); // Điều hướng sau khi đăng nhập thành công
        }

    }
}
