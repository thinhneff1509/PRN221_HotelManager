using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project_Group5.Pages.User_.ViewCart
{
    [Authorize(Roles = "Customer")]
    public class ViewCartModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public ViewCartModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        // Danh sách các booking và thanh toán của người dùng
        public IList<Booking> BookedRooms { get; set; }

        public async Task OnGetAsync(int customerId = 1)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (String.IsNullOrEmpty(userId))
            {
                customerId = int.Parse(userId);
            }
            Console.WriteLine("Debug: CustomerId = " + customerId);

            // Truy vấn các Booking của khách hàng bao gồm cả thông tin Room, RoomType, Payment, và Service
            BookedRooms = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.Roomtype) // Lấy thông tin loại phòng
                .Include(b => b.Payments)
                .Include(b => b.ServiceRegistrations)
                    .ThenInclude(sr => sr.Service) // Lấy thông tin dịch vụ đã đặt
                .Where(b => b.CustomerId == customerId)
                .ToListAsync();

        }
    }
}
