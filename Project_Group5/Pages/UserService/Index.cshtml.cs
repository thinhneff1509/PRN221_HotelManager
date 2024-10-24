using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public Customer User { get; set; }


        public List<Customer> Users { get; set; } = new();
        public void OnGet()
        {
            Users = _context.Customers.ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Thêm user mới vào database
            _context.Customers.Add(User);
             await _context.SaveChangesAsync();
           

            return RedirectToPage(); // Tải lại trang sau khi thêm user thành công
        }
    }
}
