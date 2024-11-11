using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Group5.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            _configuration = configuration ;

            _context = context ;
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

        public async Task<IActionResult> OnGetAsync()
        {

            Console.WriteLine("SelectedRooms in Checkout: " + TempData["SelectedRooms"]);

            // 1. Kiểm tra và đọc dữ liệu từ TempData
            if (TempData.ContainsKey("SelectedRooms"))
            {
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(TempData["SelectedRooms"]?.ToString() ?? "[]");
            }

            if (TempData.ContainsKey("TotalAmount"))
            {
                TotalAmount = Convert.ToDecimal(TempData["TotalAmount"]);
            }

            if (TempData.ContainsKey("StayDuration"))
            {
                StayDuration = Convert.ToInt32(TempData["StayDuration"]);
            }


            // 2. Kiểm tra nếu không có dữ liệu SelectedRooms thì quay lại trang PreOrder
            if (SelectedRooms == null || SelectedRooms.Count == 0)
            {
                return RedirectToPage("/BookingRoom");
            }

            // 3. Lấy thông tin người dùng nếu đã đăng nhập
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
        { "vnp_Amount", ((int)(amount * 100)).ToString() }, 
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

            Console.WriteLine("OnPostCheckout method called with payment method: " + paymentMethod);
            // Đảm bảo rằng TotalAmount có giá trị từ trang PreOrder
            if (TotalAmount <= 0 && SelectedRooms != null)
            {
                TotalAmount = SelectedRooms.Sum(room => (decimal)room.Price * StayDuration);
            }

            var username = User.Identity.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToPage("/Error");
            }

            // Tạo Booking mới và lưu vào cơ sở dữ liệu
            var booking = new Booking
            {
                CustomerId = customer.Id,
                RoomId = SelectedRooms.FirstOrDefault()?.RoomId ?? 0,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(StayDuration),
                TotalAmount = TotalAmount.ToString(),
                PaymentStatus = "Chờ thanh toán",
                Status = "Chờ thanh toán"
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                BookingId = booking.Id, // Liên kết BookingId từ booking đã tạo
                PaymentDate = DateTime.Now,
                Amount = TotalAmount.ToString(),
                PaymentMethod = paymentMethod,
                Status = "Chờ thanh toán",
                CheckIn = DateTime.Now,
                CheckOut = DateTime.Now.AddDays(StayDuration)
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            if (paymentMethod == "Paypal")
            {
                TempData["PaymentMessage"] = "Thanh toán thành công qua Paypal!";
                return RedirectToPage("/User_/ViewCart/ViewCart");
            }
            else
            {
                string paymentUrl = CreateVnPayUrl(TotalAmount);
                return Redirect(paymentUrl);
            }
        }

       /* public async Task<IActionResult> OnPostAsyncVNPAY()
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

            // Tạo đối tượng Booking mới
            var booking = new Booking
            {
                CustomerId = customer.Id, // Lấy ID của khách hàng đã đăng nhập
                RoomId = SelectedRooms.FirstOrDefault()?.RoomId ?? 0, // Nếu có phòng đã chọn, lấy RoomId
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(StayDuration),
                TotalAmount = TotalAmount.ToString(),
                PaymentStatus = "Pending",
                Status = "Processing"
            };

            // Lưu thông tin Booking vào database
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("Homepage/Home");
        }*/
    }

}

