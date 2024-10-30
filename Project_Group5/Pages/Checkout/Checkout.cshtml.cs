using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project_Group5.Models;

namespace Project_Group5.Pages.Checkout
{
    public class CheckoutModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public CheckoutModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        // Thuộc tính chứa thông tin thanh toán
        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string status { get; set; }

        public decimal TotalAmount { get; set; }

        public IActionResult OnGet()
        {
            TotalAmount = CalculateTotalAmount();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var order = new Booking
            {
                CustomerId = 1, // ID khách hàng đã đăng nhập (giả sử 1 là ID của khách hiện tại)
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1), // Giả sử đơn hàng trong 1 ngày
                TotalAmount = TotalAmount.ToString(),
                PaymentStatus = "Pending",
                Status = "Processing"
            };

            _context.Bookings.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Confirmation");
        }

        private decimal CalculateTotalAmount()
        {
            return 2210.00M; 
        }
    }
}

