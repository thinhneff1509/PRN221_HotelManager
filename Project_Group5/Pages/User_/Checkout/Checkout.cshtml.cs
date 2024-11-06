using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_Group5.Pages.Checkout
{
    public class CheckoutModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public CheckoutModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public List<RoomData> SelectedRooms { get; set; } = new List<RoomData>();

        public int StayDuration { get; set; }
        public decimal TotalAmount { get; set; }

        public async Task<IActionResult> OnGetAsync(string selectedRooms, string totalAmount, int stayDuration)
        {
            // Deserialize RoomData JSON from the query parameter
            if (!string.IsNullOrEmpty(selectedRooms))
            {
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(selectedRooms);
            }

            // Parse and assign TotalAmount
            TotalAmount = string.IsNullOrEmpty(totalAmount) ? 0 : decimal.Parse(totalAmount);
            StayDuration = stayDuration;

            // Kiểm tra nếu người dùng đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                // Lấy Username từ thông tin người dùng đăng nhập
                string username = User.Identity.Name;

                // Tìm kiếm thông tin khách hàng dựa vào Username
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

                if (customer != null)
                {
                    // Đổ dữ liệu khách hàng vào các thuộc tính BindProperty
                    FullName = customer.Name;
                    PhoneNumber = customer.Phone;
                    Email = customer.Email;
                    Address = customer.Address;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Xử lý logic tạo đơn hàng ở đây
            var booking = new Booking
            {
                CustomerId = 1, // Thay bằng ID khách hàng đã đăng nhập
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(StayDuration),
                TotalAmount = TotalAmount.ToString(),
                PaymentStatus = "Pending",
                Status = "Processing"
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Confirmation");
        }
    }
}
