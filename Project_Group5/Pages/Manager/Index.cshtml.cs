using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PetShop_Project_SWP391.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetShop_Project_SWP391.Pages.Manager
{
    [Authorize(Roles = "1")]
    public class IndexModel : PageModel
    {
        private readonly ProjectContext projectContext;

        public IndexModel(ProjectContext projectContext)
        {
            this.projectContext = projectContext;
        }

        [BindProperty]
        public HashSet<int> Years { get; set; }
        public double TotalWeeklySales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalGuests { get; set; }
        public int Total { get; set; }
        public int NewCustomersInMonth { get; set; }
        public long jan { get; set; }
        public long feb { get; set; }
        public long mar { get; set; }
        public long apr { get; set; }
        public long may { get; set; }
        public long jun { get; set; }
        public long jul { get; set; }
        public long aug { get; set; }
        public long sep { get; set; }
        public long oct { get; set; }
        public long nov { get; set; }
        public long dec { get; set; }

        public async Task<IActionResult> OnGet(int? Year)
        {
            if (HttpContext.Session.GetString("PetSession") == null)
            {
                return RedirectToPage("/account/singnin");
            }

            if (Year is null) Year = DateTime.Now.Year;

            Years = projectContext.Orders.Select(x => x.OrderDate.Value.Year).ToHashSet();

            // Calculate the start and end dates for the current week
            DateTime FirstDayOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime LastDayOfWeek = FirstDayOfWeek.AddDays(6);

            // Query the database to get the total weekly sales for the current week
            TotalWeeklySales = (double)await projectContext.Orders
                .Where(order => order.OrderDate >= FirstDayOfWeek && order.OrderDate <= LastDayOfWeek && order.OrderStatus == "Delivered")
                .SelectMany(order => order.OrderDetails)
                .SumAsync(orderDetail => orderDetail.Quantity * orderDetail.UnitPrice);

            TotalOrders = projectContext.Orders.Count();

            //TotalCustomers
            TotalCustomers = projectContext.Accounts.Where(x => x.CustomerId != null).Count();

            //TotalGuests
            TotalGuests = projectContext.Customers.Count() - TotalCustomers;

            //OrderInMonth
            GetOrderInMonthToStatisticSale(Year);

            //Total
            Total = await projectContext.Customers.CountAsync();

            DateTime FirstDayOfMonth = GetFirstDayOfMonth(DateTime.Now);
            NewCustomersInMonth = await projectContext.Customers.Where(x => x.CreateDate > FirstDayOfMonth).CountAsync();

            ViewData["Year"] = Year;

            return Page();
        }

        public static DateTime GetFirstDayOfMonth(DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        private void GetOrderInMonthToStatisticSale(int? Year)
        {
            jan = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 1).Count();
            feb = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 2).Count();
            mar = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 3).Count();
            apr = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 4).Count();
            may = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 5).Count();
            jun = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 6).Count();
            jul = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 7).Count();
            aug = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 8).Count();
            dec = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 9).Count();
            oct = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 10).Count();
            sep = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 11).Count();
            dec = projectContext.Orders.Where(x => ((DateTime)x.OrderDate).Year == Year && ((DateTime)x.OrderDate).Month == 12).Count();
        }
    }
}
