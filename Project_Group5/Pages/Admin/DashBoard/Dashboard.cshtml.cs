using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_Group5.Pages.Admin.DashBoard
{
    [Authorize(Roles = "Admin")]

    public class DashboardModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public DashboardModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public List<int> MonthlyRevenue { get; set; } = new List<int>();
        public List<int> MonthlyBookings { get; set; } = new List<int>();

        public void OnGet()
        {
            // Lấy dữ liệu doanh thu hàng tháng và số đơn đặt phòng hàng tháng
            MonthlyRevenue = GetMonthlyRevenue();
            MonthlyBookings = GetMonthlyBookings();
        }

        private List<int> GetMonthlyRevenue()
        {
            var monthlyRevenue = new int[12];
            var payments = _context.Payments.Where(p => p.PaymentDate.Year == DateTime.Now.Year);

            foreach (var payment in payments)
            {
                int month = payment.PaymentDate.Month - 1;
                if (int.TryParse(payment.Amount, out int amount))
                {
                    monthlyRevenue[month] += amount;
                }
            }

            return monthlyRevenue.ToList();
        }

        private List<int> GetMonthlyBookings()
        {
            var monthlyBookings = new int[12];
            var payments = _context.Payments.Where(p => p.PaymentDate.Year == DateTime.Now.Year);

            foreach (var payment in payments)
            {
                int month = payment.PaymentDate.Month - 1;
                monthlyBookings[month]++;
            }

            return monthlyBookings.ToList();
        }
    }
}
