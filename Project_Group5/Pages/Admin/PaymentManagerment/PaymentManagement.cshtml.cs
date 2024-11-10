using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
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
        public const int PageSize = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public IList<Payment> Payments { get; set; }

        [BindProperty]
        public Payment Payment { get; set; }

        // Thuộc tính tìm kiếm
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; }

        public async Task OnGetAsync(int pageIndex = 1, string sortOrder = "Newest")
        {
            CurrentPage = pageIndex;

            // Tạo query để có thể áp dụng cả filter và phân trang
            var query = _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Customer)
                .AsQueryable();

            // Sắp xếp theo PaymentDate dựa trên sortOrder
            query = sortOrder == "Newest"
                ? query.OrderByDescending(p => p.PaymentDate)
                : query.OrderBy(p => p.PaymentDate);

            // Lấy tổng số bản ghi (sau khi áp dụng filter nếu có)
            TotalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            // Lấy danh sách thanh toán với phân trang
            Payments = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Phương thức Post Edit (không cần thay đổi)
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

            return RedirectToPage(new { pageIndex = CurrentPage, searchQuery = SearchQuery });
        }
    }
}
