using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;
using Project_Group5.Models; // Thay bằng namespace của bạn
using Microsoft.EntityFrameworkCore;

namespace Project_Group5.Pages.ServiceOrder
{
    public class ServiceOrderModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public ServiceOrderModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public int BookingId { get; set; }
        public List<Service> Services { get; set; } = new List<Service>(); // Lưu danh sách dịch vụ

        [BindProperty]
        public int ServiceId { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }


        // Lấy danh sách dịch vụ và thông tin booking hiện tại
        public async Task OnGetAsync(int bookingId)
        {
            BookingId = bookingId;
            Services = await _context.Services.Where(s => s.Status == "Available").ToListAsync();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == BookingId);

            if (booking != null)
            {
                TotalAmount = decimal.Parse(booking.TotalAmount ?? "0");
            }
        }

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            var booking = await _context.Bookings
                .Include(b => b.ServiceRegistrations)
                .FirstOrDefaultAsync(b => b.Id == BookingId);

            var service = await _context.Services.FindAsync(ServiceId);

            if (booking == null || service == null)
            {
                return NotFound();
            }

            decimal servicePrice = decimal.Parse(service.Price);
            decimal totalPrice = servicePrice * Quantity;

            var serviceRegistration = new ServiceRegistration
            {
                BookingId = BookingId,
                ServiceId = ServiceId,
                Quantity = Quantity,
                TotalPrice = totalPrice
            };
            booking.ServiceRegistrations.Add(serviceRegistration);

            booking.TotalAmount = (decimal.Parse(booking.TotalAmount ?? "0") + totalPrice).ToString();

            await _context.SaveChangesAsync();

            return RedirectToPage(new { bookingId = BookingId }); // Cập nhật lại trang sau khi thêm dịch vụ
        }
    }
}
