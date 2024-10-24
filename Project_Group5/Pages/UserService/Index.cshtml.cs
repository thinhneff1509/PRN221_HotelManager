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
        public List<Customer> Users { get; set; } = new();
        public void OnGet()
        {
            Users = _context.Customers.ToList();
        }
    }
}
