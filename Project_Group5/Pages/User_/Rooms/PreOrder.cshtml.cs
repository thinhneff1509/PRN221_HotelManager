using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Project_Group5.Pages.Rooms
{
    public class PreOrderModel : PageModel
    {
        [BindProperty]
        public string CheckinDate { get; set; }

        [BindProperty]
        public string CheckoutDate { get; set; }

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
        [TempData]
        public string SelectedRoomsJson { get; set; }
        public Customer LoggedInCustomer { get; set; }


        public List<RoomData> SelectedRooms
        {
            get => JsonSerializer.Deserialize<List<RoomData>>(SelectedRoomsJson ?? "[]");
            set => SelectedRoomsJson = JsonSerializer.Serialize(value);
        }

        public async Task<IActionResult> OnGet(string checkinDate, string checkoutDate, string roomData)
        {
            if (checkinDate != null)
            {
                CheckinDate = checkinDate;
            }
            if (checkoutDate != null)
            {
                CheckoutDate = checkoutDate;
            }
            if (roomData != null)
            {
                SelectedRoomsJson = roomData;
            }
            if (DateOnly.TryParse(CheckinDate, out var checkInDate) && DateOnly.TryParse(CheckoutDate, out var checkOutDate))
            {
                TimeSpan dateDifference = checkOutDate.ToDateTime(new TimeOnly()) - checkInDate.ToDateTime(new TimeOnly());

                StayDuration = dateDifference.Days;
            }
            // Deserialize RoomData JSON into a list of RoomData objects
            SelectedRooms = JsonSerializer.Deserialize<List<RoomData>>(SelectedRoomsJson);
            var selectedRooms = SelectedRooms;
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                var RoomTypes = await context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
                foreach (var r in selectedRooms)
                {
                    r.AvailableRoom = context.Rooms.Where(rl => rl.Status == "Còn phòng" && r.Id == rl.RoomtypeId).Count();
                }
            }
            SelectedRooms = selectedRooms;
            return Page();
        }

        public Task<IActionResult> OnPostChangeBookingInfo(int roomId, int roomTypeId, int AdultCount, int ChildrenCount, string checkinDate, string checkoutDate)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            var selectedRooms = SelectedRooms;

            if (SelectedRooms == null)
            {
                return Task.FromResult<IActionResult>(BadRequest("Invalid room data."));
            }

            using (var context = new Fall24_SE1745_PRN221_Group5Context())
            {

                // Locate the corresponding RoomData in SelectedRooms
                var roomGroup = selectedRooms.FirstOrDefault(r => r.Id == roomTypeId);

                if (roomGroup == null)
                {
                    return Task.FromResult<IActionResult>(BadRequest("Room group data not found."));
                }
                var room = roomGroup.RoomList.FirstOrDefault(r => r.RoomId == roomId);

                if (roomGroup == null)
                {
                    return Task.FromResult<IActionResult>(BadRequest("Room not found."));
                }

                room.AdultCount = AdultCount;
                room.ChildrenCount = ChildrenCount;
            }
            SelectedRooms = selectedRooms;
            return Task.FromResult<IActionResult>(Page());
        }

        public async Task<IActionResult> OnPostChangeRoomNum(int roomTypeId, int roomCount, string checkinDate, string checkoutDate)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            // Deserialize the room data from JSON input
            var selectedRooms = SelectedRooms;

            if (selectedRooms == null)
            {
                return BadRequest("Invalid room data.");
            }

            using (var context = new Fall24_SE1745_PRN221_Group5Context())
            {
                // Fetch the specified RoomType with its rooms and images
                var roomType = await context.RoomTypes
                    .Include(r => r.Rooms.Where(r => r.Status == "Còn phòng"))
                    .ThenInclude(r => r.ImageRooms)
                    .FirstOrDefaultAsync(r => r.Id == roomTypeId);

                if (roomType == null)
                {
                    return NotFound($"RoomType with ID {roomTypeId} not found.");
                }

                // Locate the corresponding RoomData in SelectedRooms
                var roomGroup = selectedRooms.FirstOrDefault(r => r.Id == roomTypeId);

                if (roomGroup == null)
                {
                    return BadRequest("Room group data not found.");
                }

                // Calculate the difference in room count
                int numberOfRoomChange = roomCount - roomGroup.RoomList.Count;

                if (numberOfRoomChange > 0)
                {
                    // Add rooms to reach desired count
                    var availableRooms = roomType.Rooms
                        .Where(r => !roomGroup.RoomList.Any(sr => sr.RoomId == r.Id))
                        .Take(numberOfRoomChange)
                        .ToList();

                    roomGroup.RoomList.AddRange(availableRooms.Select(r => new SelectedRoom
                    {
                        RoomTypeId = roomType.Id,
                        RoomId = r.Id,
                        Name = r.RoomNumber,
                        RoomType = roomType.Name,
                        Price = roomType.Price,
                        Bed = roomType.Bed,
                        AdultCount = 1
                    }));
                }
                else if (numberOfRoomChange < 0)
                {
                    // Remove rooms to reach desired count
                    roomGroup.RoomList.RemoveAll(sr => sr.RoomTypeId == roomTypeId && numberOfRoomChange++ < 0);
                }
            }
            SelectedRooms = selectedRooms;
            return Page();
        }


        public async Task<IActionResult> OnPostPreOrderAsync()
        {
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    // Fetch user information from the database using their ID
                    var customer = await context.Customers.FindAsync(int.Parse(userId));
                    if (customer != null)
                    {
                        // Pre-fill the information for the logged-in user
                        Name = customer.Name;
                        Phone = customer.Phone;
                        Email = customer.Email;
                        Dob = customer.Dob;
                    }
                }
                else
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
                }


                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (SelectedRoomsJson != null)
                {
                    // Deserialize RoomData JSON into a list of RoomData objects
                    var selectedRooms = SelectedRooms;

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
                    foreach (var r in selectedRooms)
                    {
                        foreach (var selectedRoom in r.RoomList)
                        {
                            // Retrieve the room from the database using RoomId
                            var room = await context.Rooms.FirstOrDefaultAsync(ri => ri.Id == selectedRoom.RoomId);
                            if (room != null)
                            {
                                room.Status = "Đang đặt cọc";
                                context.Update(room);

                                // Calculate discount if applicable
                                /*                                var discount = await context.Discounts.FirstOrDefaultAsync(d => d.BookingId == room.Id);*/
                                double finalAmount = double.Parse(TotalAmount);
                                /*                                if (discount != null)
                                                                {   
                                                                    // Assuming discount is a percentage
                                                                    finalAmount = finalAmount * (1 - (discount.Amount / 100));
                                                                }
                                */
                                // Create booking with updated amount
                                var booking = new Booking
                                {
                                    CustomerId = customer.Id,
                                    CheckInDate = CheckInDate,
                                    CheckOutDate = CheckOutDate,
                                    Status =
                                    TotalAmount = finalAmount.ToString(), // Store the total after applying discount
                                    RoomId = room.Id
                                };
                                context.Add(booking);
                            }
                        }
                    }
                    SelectedRooms = selectedRooms;

                    await context.SaveChangesAsync();

                    //Simulate log out
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    return RedirectToPage("/Homepage/Home");
                }
            }
            return Page();
        }
    }
}