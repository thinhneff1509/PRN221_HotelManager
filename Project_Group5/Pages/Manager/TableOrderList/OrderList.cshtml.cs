using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PetShop_Project_SWP391.Pages.Manager.TableOrderList
{
    [Authorize(Roles = "1")]
    public class OrderListModel : PageModel
    {
        private readonly ProjectContext _projectContext;

        public OrderListModel(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }
        [BindProperty(SupportsGet = true)]
        public string OrderStatusFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        public PaginatedList<Order> Orders { get; set; }

        public async Task<IActionResult> OnGet(int? pageIndex)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/signin");
            }

            int pageSize = 10;
            IQueryable<Order> ordersQuery = _projectContext.Orders.Include(o => o.Customer);

            if (!string.IsNullOrEmpty(OrderStatusFilter))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == OrderStatusFilter);
            }

            if (StartDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= StartDate);
            }

            if (EndDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= EndDate);
            }
            Orders = await PaginatedList<Order>.CreateAsync(ordersQuery.AsNoTracking(), pageIndex ?? 1, pageSize);

            return Page();
        }

        public IActionResult OnGetDetail(int orderId)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/singnin");
            }
            else
            {
                return RedirectToPage("Detail", new { orderId = orderId });
            }
        }
    }
}
