using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Service> Services { get; set; } = new List<Service>();
        public List<ServiceRegistration> ServiceRegistrations { get; set; } = new List<ServiceRegistration>();

        [BindProperty]
        public int RoomId { get; set; }  // ID phòng sẽ nhận dịch vụ
        [BindProperty]
        public int ServiceId { get; set; }
        [BindProperty]
        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public async Task OnGetAsync(int bookingId)
        {
            BookingId = bookingId;

            // Lấy danh sách phòng và dịch vụ từ cơ sở dữ liệu
            Rooms = await _context.Rooms.Include(r => r.Roomtype).ToListAsync();
            Services = await _context.Services.Where(s => s.Status == "Available").ToListAsync();

            // Truy vấn ServiceRegistrations, bao gồm Booking và Room qua Booking
            ServiceRegistrations = await _context.ServiceRegistrations
                .Include(sr => sr.Service)
                .Include(sr => sr.Booking)
                    .ThenInclude(b => b.Room) // Truy cập Room thông qua Booking
                .Where(sr => sr.BookingId == BookingId)
                .ToListAsync();

            TotalAmount = ServiceRegistrations.Sum(sr => sr.TotalPrice ?? 0);
        }


        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            var booking = await _context.Bookings
                .Include(b => b.ServiceRegistrations)
                 .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == BookingId);

            var service = await _context.Services.FindAsync(ServiceId);

            if (booking == null || service == null)
            {
                return NotFound();
            }

            decimal servicePrice = decimal.Parse(service.Price);
            decimal totalPrice = servicePrice * Quantity;

            // Tạo đối tượng ServiceRegistration mới cho phòng đã chọn
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

            return RedirectToPage(new { bookingId = BookingId });
        }
    }
}
