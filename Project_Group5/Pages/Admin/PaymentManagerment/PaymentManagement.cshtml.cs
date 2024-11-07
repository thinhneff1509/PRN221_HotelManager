using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_Group5.Pages.Admin.PaymentManagement
{
    public class PaymentManagementModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public PaymentManagementModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        // Các tham số phân trang
        public const int PageSize = 10;  // Số mục mỗi trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public IList<Payment> Payments { get; set; }

        [BindProperty]
        public Payment Payment { get; set; }

        public async Task OnGetAsync(int pageIndex = 1)
        {
            CurrentPage = pageIndex;

            // Lấy tổng số bản ghi thanh toán
            TotalItems = await _context.Payments.CountAsync();

            // Tính tổng số trang
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            // Lấy danh sách thanh toán với phân trang
            Payments = await _context.Payments
                .Include(p => p.Booking).ThenInclude(p => p.Room)
                .OrderBy(p => p.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var paymentToUpdate = await _context.Payments.FindAsync(Payment.Id);
            if (paymentToUpdate == null)
            {
                return NotFound();
            }

            paymentToUpdate.PaymentDate = DateTime.Now;
            paymentToUpdate.Amount = Payment.Amount;
            paymentToUpdate.PaymentMethod = Payment.PaymentMethod;
            paymentToUpdate.Status = Payment.Status;

            await _context.SaveChangesAsync();

            // Thêm thông báo thành công vào TempData
            TempData["SuccessMessage"] = "Cập nhật thanh toán thành công!";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { pageIndex = CurrentPage });
        }
    }
}
