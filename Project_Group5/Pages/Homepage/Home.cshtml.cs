using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;

namespace Project_Group5.Pages.Homepage
{
    public class HomeModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public HomeModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }
    }
}
