using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Threading.Tasks;

namespace Project_Group5.Pages.ProfileCustomer
{
    public class ProfileModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public ProfileModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; }

        [BindProperty]
        public bool IsEditMode { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var username = User.Identity.Name;
            Customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (Customer == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPostEditMode()
        {
            IsEditMode = true;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                IsEditMode = true; // Duy trì chế độ chỉnh sửa nếu có lỗi
                return Page();
            }

            var customerToUpdate = await _context.Customers.FindAsync(Customer.Id);

            if (customerToUpdate == null)
            {
                return NotFound();
            }

            customerToUpdate.Name = Customer.Name;
            customerToUpdate.Email = Customer.Email;
            customerToUpdate.Dob = Customer.Dob;
            customerToUpdate.Phone = Customer.Phone;
            customerToUpdate.Address = Customer.Address;

            await _context.SaveChangesAsync();

            IsEditMode = false; // Quay lại chế độ xem sau khi lưu
            return RedirectToPage(); // Quay về trang hiện tại nhưng ở chế độ xem
        }
    }
}
