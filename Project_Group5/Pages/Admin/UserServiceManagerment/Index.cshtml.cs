using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace Project_Group5.Pages.Admin.UserServiceManagerment
{
    public class IndexModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Role> Roles { get; set; }

        // Phương thức OnGetAsync với lọc theo role
        public async Task OnGetAsync(string searchString, int? roleFilter)
        {
            // Lấy tất cả các vai trò để hiển thị trong dropdown
            Roles = await _context.Roles.ToListAsync();
           
            // Lấy danh sách khách hàng theo bộ lọc
            Customers = await _context.Customers
                .Where(c => (string.IsNullOrEmpty(searchString) ||
                            c.Name.Contains(searchString) ||
                            c.Phone.Contains(searchString) ||
                            c.Email.Contains(searchString)) &&
                            (!roleFilter.HasValue || c.RoleId == roleFilter)) // Lọc theo role nếu roleFilter có giá trị
                .ToListAsync();
        }

        // Thêm, cập nhật và xóa giữ nguyên như cũ
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _context.Roles.ToListAsync();
                ViewData["ShowModal"] = true;
                Customers = await _context.Customers.ToListAsync();
                return Page();
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == Customer.Email);

            if (existingCustomer != null)
            {
                ModelState.AddModelError("Customer.Email", "Email đã tồn tại. Vui lòng nhập email khác.");
                Roles = await _context.Roles.ToListAsync();
                ViewData["ShowModal"] = true;
                Customers = await _context.Customers.ToListAsync();
                return Page();
            }

            _context.Customers.Add(Customer);
            await _context.SaveChangesAsync();

            // Thêm thông báo thành công
            TempData["SuccessMessage"] = "Người dùng đã được thêm thành công.";

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var customerToUpdate = await _context.Customers.FindAsync(Customer.Id);
            if (customerToUpdate == null)
            {
                return NotFound();
            }

            customerToUpdate.Name = Customer.Name;
            customerToUpdate.Phone = Customer.Phone;
            customerToUpdate.RoleId = Customer.RoleId;

            await _context.SaveChangesAsync();

            // Thêm thông báo thành công
            TempData["SuccessMessage"] = "Người dùng đã được cập nhật thành công.";

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customerToDelete = await _context.Customers.FindAsync(id);
            if (customerToDelete == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customerToDelete);
            await _context.SaveChangesAsync();

            // Thêm thông báo thành công
            TempData["SuccessMessage"] = "Người dùng đã được xóa thành công.";

            return RedirectToPage();
        }

    }
}
