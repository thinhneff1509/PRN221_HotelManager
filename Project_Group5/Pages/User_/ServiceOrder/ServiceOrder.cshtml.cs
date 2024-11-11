using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Numerics;
using System.Security.Claims;
using System.Xml.Linq;

namespace Project_Group5.Pages.ServiceOrder
{
    //[Authorize(Roles = "1,2")]
    public class ServiceOrderModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public ServiceOrderModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public int BookingId { get; set; }
        [BindProperty]
        public List<Booking> bookings { get; set; }
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Service> Services { get; set; } = new List<Service>();
        public List<ServiceRegistration> ServiceRegistrations { get; set; } = new List<ServiceRegistration>();

        [BindProperty]
        public int ServiceId { get; set; }
        [BindProperty]
        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public async Task OnGetAsync(int bookingId)
        {
            BookingId = bookingId;

            bookings = await GetListBooking();


            // Lấy danh sách phòng và dịch vụ từ cơ sở dữ liệu
            Rooms = await _context.Rooms.Include(r => r.Roomtype).ToListAsync();
            Services = await _context.Services.Where(s => s.Status == "Available").ToListAsync();

/*            // Lấy các dịch vụ đã đăng ký cho booking này và tính tổng tiền
            ServiceRegistrations = await _context.ServiceRegistrations
                .Include(sr => sr.Service)
                .Include(sr => sr.Booking)
                    .ThenInclude(b => b.Room) // Bao gồm Room thông qua Booking
                .Where(sr => sr.BookingId == BookingId)
                .ToListAsync();

            TotalAmount = ServiceRegistrations.Sum(sr => sr.TotalPrice ?? 0);*/
        }

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            var booking = await _context.Bookings
                .Include(b => b.Room) // Bao gồm Room qua Booking
                .Include(b => b.ServiceRegistrations)
                .FirstOrDefaultAsync(b => b.Id == BookingId);

            var service = await _context.Services.FindAsync(ServiceId);

            if (booking == null || service == null)
            {
                return NotFound();
            }

            decimal servicePrice = decimal.Parse(service.Price);
            decimal totalPrice = servicePrice * Quantity;

            // Tạo đối tượng ServiceRegistration mới và thêm vào Booking
            var serviceRegistration = new ServiceRegistration
            {
                BookingId = BookingId,
                ServiceId = ServiceId,
                Quantity = Quantity,
                TotalPrice = totalPrice
            };

            booking.ServiceRegistrations.Add(serviceRegistration);

            // Cập nhật tổng tiền của booking
            booking.TotalAmount = (decimal.Parse(booking.TotalAmount ?? "0") + totalPrice).ToString();

            await _context.SaveChangesAsync();

            return RedirectToPage("/User_/ViewCart/ViewCart");
        }

        public async Task<List<Booking>> GetListBooking()
        {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Customer customer = new Customer();
                if (userId != null)
                {
                    // Fetch user information from the database using their ID
                    customer = await _context.Customers.FindAsync(int.Parse(userId));
                }
               
                return await _context.Bookings.Include(b => b.Room).ThenInclude(b => b.Roomtype).Where(c => c.CustomerId == customer.Id && c.CheckInDate <= DateTime.Now && c.CheckOutDate >= DateTime.Now).ToListAsync();
        }
    }
}
