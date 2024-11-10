using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Group5.Models;
using System.Security.Cryptography;
using System.Text;

namespace Project_Group5.Pages.Checkout
{
    [Authorize(Roles = "Customer")] // Yêu cầu người dùng phải đăng nhập
    public class CheckoutModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        private readonly IConfiguration _configuration;


        // Inject IConfiguration và DbContext vào lớp thông qua constructor
        public CheckoutModel(IConfiguration configuration, Fall24_SE1745_PRN221_Group5Context context)
        {
            // Lưu trữ IConfiguration vào biến _configuration để sử dụng sau này
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Lưu trữ DbContext vào biến _context để làm việc với cơ sở dữ liệu
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

            // Parse StayDuration and assign
            StayDuration = stayDuration;

            // Kiểm tra nếu người dùng đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity.Name;
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

                if (customer != null)
                {
                    FullName = customer.Name;
                    PhoneNumber = customer.Phone;
                    Email = customer.Email;
                    Address = customer.Address;

                    // Tìm booking của khách hàng để lấy `TotalAmount` từ Payment
                    var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.CustomerId == customer.Id);
                    if (booking != null)
                    {
                        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == booking.Id);
                        if (payment != null)
                        {
                            // Chuyển đổi payment.Amount từ string sang decimal
                            if (decimal.TryParse(payment.Amount, out decimal amount))
                            {
                                TotalAmount = amount;
                            }
                            else
                            {
                                // Xử lý nếu không chuyển đổi được, ví dụ: gán giá trị mặc định hoặc log lỗi
                                TotalAmount = 0;
                            }
                        }
                    }
                }
            }

            return Page();
        }


        public string CreateVnPayUrl(decimal amount)
        {
            // Lấy cấu hình từ appsettings.json
            string baseUrl = _configuration["VnpayConfig:vnp_Url"];
            string returnUrl = _configuration["VnpayConfig:vnp_ReturnUrl"];
            string tmnCode = _configuration["VnpayConfig:vnp_TmnCode"];
            string hashSecret = _configuration["VnpayConfig:vnp_HashSecret"];

            var vnpay = new Dictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", tmnCode },
        { "vnp_Amount", ((int)(amount * 100)).ToString() }, // VNPAY yêu cầu amount là đơn vị VNĐ x100
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", DateTime.Now.Ticks.ToString() }, // ID giao dịch duy nhất
        { "vnp_OrderInfo", "Thanh toán đơn hàng" },
        { "vnp_Locale", "vn" },
        { "vnp_ReturnUrl", returnUrl },
        { "vnp_IpAddr", HttpContext.Connection.RemoteIpAddress.ToString() },
        { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
    };

            // Tạo chuỗi query
            string query = string.Join("&", vnpay.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            string signData = query + hashSecret;

            // Tạo chữ ký bảo mật
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                query += $"&vnp_SecureHash={hashString}";
            }

            // Trả về URL thanh toán
            return $"{baseUrl}?{query}";
        }

        public async Task<IActionResult> OnPostCheckout(string paymentMethod)
        {
            // Kiểm tra xem TotalAmount có giá trị chưa, nếu không, hãy gán giá trị từ Model hoặc từ cơ sở dữ liệu
            // Đảm bảo rằng `TotalAmount` có giá trị
            if (TotalAmount <= 0 && SelectedRooms != null)
            {
                // Tính tổng số tiền dựa trên các phòng đã chọn và thời gian ở
                TotalAmount = SelectedRooms.Sum(room => (decimal)room.Price * StayDuration);
            }

            var username = User.Identity.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToPage("/Error");
            }

            // Tạo Booking mới
            var booking = new Booking
            {
                CustomerId = customer.Id,
                RoomId = SelectedRooms.FirstOrDefault()?.RoomId ?? 0,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(StayDuration),
                TotalAmount = TotalAmount.ToString(),
                PaymentStatus = "Completed",  // Đánh dấu thanh toán hoàn tất
                Status = "Processed"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Tạo Payment mới với phương thức thanh toán được chọn
            var payment = new Payment
            {
                BookingId = booking.Id,
                PaymentDate = DateTime.Now,
                Amount = TotalAmount.ToString(),
                PaymentMethod = paymentMethod, // Lưu phương thức thanh toán
                Status = "Completed",  // Trạng thái thanh toán hoàn tất
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(StayDuration)
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();


            // Tạo URL thanh toán và chuyển hướng đến VNPAY
            string paymentUrl = CreateVnPayUrl(TotalAmount);
            return Redirect(paymentUrl);

            // Thông báo thanh toán thành công và chuyển hướng về trang chủ
            TempData["PaymentMessage"] = "Thanh toán thành công!";

            return RedirectToPage("/User_/ViewCart");

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Giả sử bạn đang lưu Username trong `User.Identity.Name`
            string username = User.Identity.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                // Nếu không tìm thấy khách hàng, bạn có thể xử lý lỗi tại đây
                return RedirectToPage("/Error");
            }

            foreach (var r in SelectedRooms)
            {
                var booking = new Booking
                {
                    CustomerId = customer.Id, // Lấy ID của khách hàng đã đăng nhập
                    RoomId = r.RoomTypeId, // Nếu có phòng đã chọn, lấy RoomId
                    CheckInDate = DateTime.Now,
                    CheckOutDate = DateTime.Now.AddDays(StayDuration),
                    TotalAmount = TotalAmount.ToString(),
                    PaymentStatus = "Pending",
                    Status = "Processing"
                };
                _context.Bookings.Add(booking);
            }
            // Tạo đối tượng Booking mới

            // Lưu thông tin Booking vào database
            await _context.SaveChangesAsync();

            return RedirectToPage("Homepage/Home");
        }
    }
}
