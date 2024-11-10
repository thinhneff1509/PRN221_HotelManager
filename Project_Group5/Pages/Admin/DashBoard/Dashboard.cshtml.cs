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

        public List<decimal> MonthlyRevenue { get; set; } = new List<decimal>();
        public List<int> MonthlyBookings { get; set; } = new List<int>();
        public int? SelectedMonth { get; set; } // Tháng được chọn từ front-end

        public void OnGet(int? month = null)
        {
            SelectedMonth = month;
            MonthlyRevenue = GetMonthlyRevenue(month);
            MonthlyBookings = GetMonthlyBookings(month);
        }

        private List<decimal> GetMonthlyRevenue(int? month)
        {
            var monthlyRevenue = new decimal[12];
            var payments = _context.Payments
                .Where(p => p.PaymentDate.Year == DateTime.Now.Year && (!month.HasValue || p.PaymentDate.Month == month))
                .ToList();

            foreach (var payment in payments)
            {
                int monthIndex = payment.PaymentDate.Month - 1;
                monthlyRevenue[monthIndex] += decimal.Parse(payment.Amount);
            }

            return monthlyRevenue.ToList();
        }

        private List<int> GetMonthlyBookings(int? month)
        {
            var monthlyBookings = new int[12];
            var payments = _context.Payments
                .Where(p => p.PaymentDate.Year == DateTime.Now.Year && (!month.HasValue || p.PaymentDate.Month == month))
                .ToList();

            foreach (var payment in payments)
            {
                int monthIndex = payment.PaymentDate.Month - 1;
                monthlyBookings[monthIndex]++;
            }

            return monthlyBookings.ToList();
        }
    }
}
