using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Project_Group5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_Group5.Pages.Rooms
{
    public class PreOrderModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public PreOrderModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

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
        public decimal TotalAmount { get; set; }

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

            if (!string.IsNullOrEmpty(RoomData))
            {
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(RoomData);
                CalculateTotalAmount();
            }
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = SelectedRooms.Sum(room => (decimal)(room.RoomList.Count * room.Price * StayDuration));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate inputs
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
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@") || !Email.Contains("."))
            {
                ModelState.AddModelError("Email", "Invalid email address.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(RoomData))
            {
                SelectedRooms = JsonConvert.DeserializeObject<List<RoomData>>(RoomData);
                CalculateTotalAmount();

                // Retrieve or create customer
                Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Email || c.Phone == Phone);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = Name,
                        Email = Email,
                        Phone = Phone,
                        Dob = Dob,
                        RegisterDate = DateTime.Now,
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Create bookings
                DateTime checkIn = DateTime.Parse(CheckinDate);
                DateTime checkOut = DateTime.Parse(CheckoutDate);

                foreach (var room in SelectedRooms)
                {
                    var availableRooms = await _context.Rooms
                        .Where(r => r.RoomtypeId == int.Parse(room.RoomType) && r.Status != "full")
                        .Take(room.RoomList.Count)
                        .ToListAsync();

                    foreach (var availableRoom in availableRooms)
                    {
                        availableRoom.Status = "full";
                        _context.Update(availableRoom);

                        var booking = new Booking
                        {
                            CustomerId = customer.Id,
                            RoomId = availableRoom.Id,
                            CheckInDate = checkIn,
                            CheckOutDate = checkOut,
                            TotalAmount = TotalAmount.ToString(),
                            Status = "Pending",
                            PaymentStatus = "Unpaid",
                        };
                        _context.Bookings.Add(booking);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Redirect to the Checkout page with data in query parameters
            return RedirectToPage("/Checkout", new { selectedRooms = RoomData, totalAmount = TotalAmount, stayDuration = StayDuration });
        }
    }
}
