using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PetShop_Project_SWP391.Pages.Manager.TableOrderList
{
    public class DetailModel : PageModel
    {
        private readonly ProjectContext _projectContext;

        public DetailModel(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        [BindProperty]
        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/singnin");
            }
            else
            {
                Order = await _projectContext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (Order == null)
                {
                    return NotFound();
                }

                return Page();
            }
        }

        public async Task<IActionResult> OnPostActiveAsync(int orderId, string action)
        {
            Models.Order order = _projectContext.Orders.FirstOrDefault(o => o.OrderId == orderId);
            Order = order;

            if (Order.OrderStatus == "Future")
            {
                if (action == "complete")
                {
                    Order.OrderStatus = "Complete";
                }
                else if (action == "cancel")
                {
                    Order.OrderStatus = "Cancel";
                }

                await _projectContext.SaveChangesAsync();
            }

            // Redirect back to the Order Detail page
            return RedirectToPage("Detail", new { orderId = orderId });
        }
    }
}
