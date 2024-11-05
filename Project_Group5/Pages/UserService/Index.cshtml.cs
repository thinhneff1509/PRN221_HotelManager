using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.UserService
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
        public async Task OnGetAsync(string searchString)
        {
            // Lấy tất cả khách hàng từ bảng Customer
            Customers = await _context.Customers
                .Where(c => string.IsNullOrEmpty(searchString) ||
                            c.Name.Contains(searchString) ||
                            c.Phone.Contains(searchString) ||
                            c.Email.Contains(searchString))
                .ToListAsync();

            Roles = await _context.Roles.ToListAsync();



            Roles = await _context.Roles.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _context.Roles.ToListAsync();
                ViewData["ShowModal"] = true;
                Customers = await _context.Customers.ToListAsync();
                return Page(); // Nếu dữ liệu không hợp lệ, trở lại trang
            }

            // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == Customer.Email);

            if (existingCustomer != null)
            {
                ModelState.AddModelError("Customer.Email", "Email đã tồn tại. Vui lòng nhập email khác.");
                Roles = await _context.Roles.ToListAsync();
                ViewData["ShowModal"] = true;
                Customers = await _context.Customers.ToListAsync();
                return Page(); // Trở lại trang với thông báo lỗi
            }

            _context.Customers.Add(Customer); // Thêm khách hàng mới vào DbContext
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách khách hàng
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {

            var customerToUpdate = await _context.Customers.FindAsync(Customer.Id); // Tìm khách hàng theo ID
            if (customerToUpdate == null)
            {
                return NotFound(); // Nếu không tìm thấy khách hàng
            }

            // Cập nhật thông tin khách hàng
            customerToUpdate.Name = Customer.Name;
            customerToUpdate.Phone = Customer.Phone;
            customerToUpdate.Email = Customer.Email;
            customerToUpdate.RoleId = Customer.RoleId;


            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách khách hàng
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var customerToDelete = await _context.Customers.FindAsync(id);
            if (customerToDelete == null)
            {
                return NotFound(); // Nếu không tìm thấy khách hàng
            }

            _context.Customers.Remove(customerToDelete); // Xóa khách hàng
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách khách hàng
        }


    }
}
