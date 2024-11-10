using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.Admin.DiscountManagement
{
    public class DiscountManagementModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public DiscountManagementModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Discount Discount { get; set; }
        public List<Discount> Discounts { get; set; }

        public async Task OnGetAsync(string searchString)
        {
            Discounts = await _context.Discounts
                .Where(r => string.IsNullOrEmpty(searchString) ||
                            r.Name.Contains(searchString) ||
                            r.Content.Contains(searchString))
                .ToListAsync();
        }

        // Thêm mới phòng
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["ShowModal"] = true;
                Discounts = await _context.Discounts.ToListAsync();
                return Page(); // Nếu dữ liệu không hợp lệ, trở lại trang
            }

            _context.Discounts.Add(Discount); 
            await _context.SaveChangesAsync();

            return RedirectToPage(); 
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var discountToUpdate = await _context.Discounts.FindAsync(Discount.Id);
            if (discountToUpdate == null)
            {
                return NotFound(); // Nếu không tìm thấy phòng
            }

            // Cập nhật thông tin phòng
            discountToUpdate.Name = Discount.Name;
            discountToUpdate.Amount = Discount.Amount;
            discountToUpdate.Content = Discount.Content;
            discountToUpdate.EffectiveDate = Discount.EffectiveDate;
            discountToUpdate.ExpirationDate = Discount.ExpirationDate;

            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            return RedirectToPage(); // Chuyển hướng về trang danh sách phòng
        }

        // Xóa hạng phòng
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var discountToDelete = await _context.Discounts
                .Include(b => b.Bookings)
                    .ThenInclude(b => b.ServiceRegistrations)
                .Include(b => b.Bookings)
                    .ThenInclude(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (discountToDelete == null)
            {
                return NotFound(); 
            }

            foreach (var booking in discountToDelete.Bookings)
            {
                // Delete the ServiceRegistrations associated with the Booking
                _context.ServiceRegistrations.RemoveRange(booking.ServiceRegistrations);

                // Delete the Payments associated with the Booking
                _context.Payments.RemoveRange(booking.Payments);
            }
            // Delete the Bookings associated with the RoomType
            _context.Bookings.RemoveRange(discountToDelete.Bookings);

            // Delete the RoomType
            _context.Discounts.Remove(discountToDelete);

            // Save changes
            await _context.SaveChangesAsync();

            // Set success message
            TempData["SuccessMessage"] = "Xóa hạng phòng thành công!";

            return RedirectToPage();
        }
    }
}
