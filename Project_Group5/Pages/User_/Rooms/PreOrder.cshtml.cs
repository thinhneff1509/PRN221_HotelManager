using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class PreOrderModel : PageModel
    {
        [BindProperty]
        public string CheckinDate { get; set; }

        [BindProperty]
        public string CheckoutDate { get; set; }

        [BindProperty]
        public string RoomData { get; set; }
        [BindProperty]
        public int StayDuration { get; set; }
        [BindProperty]
        public string Name { get; set; }
        [BindProperty]
        public string Phone { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public DateTime? Dob { get; set; }
        [BindProperty]
        public string TotalAmount { get; set; }
        public List<RoomData> SelectedRooms { get; set; }

        public void OnGet(string checkinDate, string checkoutDate, string roomData)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            RoomData = roomData;
            if (DateOnly.TryParse(CheckinDate, out var checkInDate) && DateOnly.TryParse(CheckoutDate, out var checkOutDate))
            {
                TimeSpan dateDifference = checkOutDate.ToDateTime(new TimeOnly()) - checkInDate.ToDateTime(new TimeOnly());

                StayDuration = dateDifference.Days;
            }
            if (roomData != null)
            {
                Console.WriteLine(roomData);
                // Deserialize RoomData JSON into a list of RoomData objects
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(RoomData);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }
            if (!Dob.HasValue)
            {
                ModelState.AddModelError("Dob", "Date of Birth is required.");
            }
            if (string.IsNullOrWhiteSpace(Phone))
            {
                ModelState.AddModelError("Phone", "Phone number is required.");
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!Email.Contains("@") || !Email.Contains("."))
            {
                ModelState.AddModelError("Email", "Invalid email address.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (RoomData != null)
            {
                // Deserialize RoomData JSON into a list of RoomData objects
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(RoomData);

                using (var context = new Fall24_SE1745_PRN221_Group5Context())
                {
                    var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email.Equals(Email) || c.Phone.Equals(Phone));
                    if (customer == null)
                    {
                        var newCustomer = new Customer
                        {
                            Name = Name,
                            Email = Email,
                            Phone = Phone,
                            Dob = Dob,
                            RegisterDate = DateTime.Now,
                        };
                        await context.Customers.AddAsync(newCustomer);
                        await context.SaveChangesAsync();
                        customer = newCustomer;
                    }
                    DateTime CheckInDate = DateTime.Parse(CheckinDate);
                    DateTime CheckOutDate = DateTime.Parse(CheckoutDate);
                    foreach (var r in SelectedRooms)
                    {
                        var listRoom = await context.Rooms.Where(lr => lr.RoomtypeId == int.Parse(r.RoomType) && !lr.Status.Equals("Đang đặt cọc")).Take(r.RoomList.Count).ToListAsync();
                        foreach (var ri in listRoom)
                        {
                            ri.Status = "Đang đặt cọc";
                            context.Update(ri);
                            // Tính toán giảm giá, nếu có
                            var discount = await context.Discounts.FirstOrDefaultAsync(d => d.BookingId == ri.Id);
                            double finalAmount = double.Parse(TotalAmount);
                            if (discount != null)
                            {
                                // Giả sử giảm giá là theo phần trăm
                                //      finalAmount = finalAmount * (1 -(discount.Amount / 100));
                            }

                            var booking = new Booking
                            {
                                CustomerId = customer.Id,
                                CheckInDate = CheckInDate,
                                CheckOutDate = CheckOutDate,
                                TotalAmount = finalAmount.ToString(), // Lưu số tiền sau khi đã áp dụng giảm giá
                                RoomId = ri.Id
                            };
                            context.Add(booking);
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }
            return RedirectToPage("/Homepage/Home");
        }
    }
}