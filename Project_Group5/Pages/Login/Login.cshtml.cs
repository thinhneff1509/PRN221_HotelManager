using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Project_Group5.Models;
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

            // Redirect based on role
            if (customer.Role.Id == 1)
            {
                return RedirectToPage("/Admin/Dashboard"); // Admin page
            }
            else
            {
                return RedirectToPage("/Homepage/Home"); // Regular user homepage
            }
        }

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Homepage/Home");
        }
    }
}
