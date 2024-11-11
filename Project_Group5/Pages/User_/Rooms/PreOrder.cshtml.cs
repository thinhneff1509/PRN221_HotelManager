using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using Decimal = System.Decimal;

namespace Project_Group5.Pages.Rooms
{
    public class PreOrderModel : PageModel
    {
        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime CheckinDate { get; set; } = DateTime.Now;

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime CheckoutDate { get; set; } = DateTime.Now.AddDays(1);

        [BindProperty] public int StayDuration { get; set; } = 1;
        [BindProperty] public string Name { get; set; }
        [BindProperty] public string Phone { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public DateTime? Dob { get; set; }
        [BindProperty] public string TotalAmount { get; set; }
        [TempData] public string SelectedRoomsJson { get; set; }
        public Customer LoggedInCustomer { get; set; }
        [BindProperty] public string PromoCode { get; set; }
        [BindProperty] public decimal Discount { get; set; }
        [BindProperty] public string SDiscount { get; set; }


        public List<RoomData> SelectedRooms
        {
            get => JsonSerializer.Deserialize<List<RoomData>>(SelectedRoomsJson ?? "[]");
            set => SelectedRoomsJson = JsonSerializer.Serialize(value);
        }

        public async Task<IActionResult> OnGet(string checkinDate, string checkoutDate, string promoCode,
            string sDiscount, string roomData)
        {
            if (checkinDate != null)
            {
                CheckinDate = DateTime.Parse(checkinDate);
            }
            else
            {
                CheckinDate = DateTime.Today;
            }

            if (checkoutDate != null)
            {
                CheckoutDate = DateTime.Parse(checkoutDate);
            }
            else
            {
                CheckinDate = DateTime.Today.AddDays(1);
            }

            if (roomData != null)
            {
                SelectedRoomsJson = roomData;
            }

            if (promoCode != null)
            {
                PromoCode = promoCode;
            }

            if (sDiscount != null)
            {
                SDiscount = sDiscount;
            }

            TimeSpan dateDifference = CheckinDate - CheckoutDate;

            StayDuration = Math.Abs(dateDifference.Days);
            if (StayDuration <= 0)
            {
                StayDuration = 1;
            }

            this.StayDuration = StayDuration;

            var selectedRooms = SelectedRooms;
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                var RoomTypes = await context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms)
                    .ToListAsync();
                foreach (var r in selectedRooms)
                {
                    r.AvailableRoom = context.Rooms
                        .Where(rl => rl.Status != "Hết phòng" && r.Id == rl.RoomtypeId).Count();
                }
            }

            decimal totalDiscountPercentage = 0;

            // Apply PromoCode discount if applicable
            if (!string.IsNullOrEmpty(PromoCode))
            {
                var discount = await GetDiscountByPromoCode(PromoCode);
                if (discount != null &&
                    decimal.TryParse(discount.Amount, out decimal discountAmount))
                {
                    totalDiscountPercentage += discountAmount;
                }
            }

            // Apply Special discount if applicable
            if (!string.IsNullOrEmpty(SDiscount))
            {
                var discount = await GetDiscountByPromoCode(SDiscount);
                if (discount != null &&
                    decimal.TryParse(discount.Amount, out decimal discountAmount))
                {
                    totalDiscountPercentage += discountAmount;
                }
            }
            this.Discount = totalDiscountPercentage;

            SelectedRooms = selectedRooms;
            this.CheckinDate = CheckinDate;
            this.CheckoutDate = CheckoutDate;
            this.StayDuration = StayDuration;
            return Page();
        }

        public async Task<IActionResult> OnPostChangeBookingInfo(int roomId, int roomTypeId, int AdultCount,
            int ChildrenCount, DateTime checkinDate, DateTime checkoutDate, Decimal Discount, string PromoCode, int SDiscount)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            this.StayDuration = StayDuration;
            var selectedRooms = SelectedRooms;
            if (SelectedRooms == null)
            {
                return Page();
            }

            using (var context = new Fall24_SE1745_PRN221_Group5Context())
            {

                // Locate the corresponding RoomData in SelectedRooms
                var roomGroup = selectedRooms.FirstOrDefault(r => r.Id == roomTypeId);

                if (roomGroup == null)
                {
                    return Page();
                }

                var room = roomGroup.RoomList.FirstOrDefault(r => r.RoomId == roomId);

                if (room == null)
                {
                    return Page();
                }

                room.AdultCount = AdultCount;
                room.ChildrenCount = ChildrenCount;
            }
            this.Discount = Discount;
            SelectedRooms = selectedRooms;
            return Page();
        }

        public async Task<IActionResult> OnPostChangeRoomNum(int roomTypeId, int roomCount, DateTime checkinDate,
            DateTime checkoutDate, Decimal Discount, string PromoCode, int SDiscount)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            this.StayDuration = StayDuration;
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
                    .Include(r => r.Rooms.Where(r => r.Status != "Hết phòng"))
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
            this.Discount = Discount;
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

                    if (!ModelState.IsValid)
                    {
                        return Page();
                    }
                }

                if (SelectedRoomsJson != null)
                {
                    // Deserialize RoomData JSON into a list of RoomData objects
                    var selectedRooms = SelectedRooms;

                    var customer =
                        await context.Customers.FirstOrDefaultAsync(c =>
                            c.Email.Equals(Email) || c.Phone.Equals(Phone));
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

                    DateTime CheckInDate = CheckinDate;
                    DateTime CheckOutDate = CheckoutDate;
                    int discountId = context.Discounts.FirstOrDefault(d => d.Name == PromoCode || d.Name == SDiscount)?.Id ?? 0;
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
                                Decimal finalAmount = Decimal.Parse(TotalAmount);

                                decimal totalDiscountPercentage = 0;

                                // Apply PromoCode discount if applicable
                                if (!string.IsNullOrEmpty(PromoCode))
                                {
                                    var discount = await GetDiscountByPromoCode(PromoCode);
                                    if (discount != null &&
                                        decimal.TryParse(discount.Amount, out decimal discountAmount))
                                    {
                                        totalDiscountPercentage += discountAmount;
                                    }
                                }

                                // Apply Special discount if applicable
                                if (!string.IsNullOrEmpty(SDiscount))
                                {
                                    var discount = await GetDiscountByPromoCode(SDiscount);
                                    if (discount != null &&
                                        decimal.TryParse(discount.Amount, out decimal discountAmount))
                                    {
                                        totalDiscountPercentage += discountAmount;
                                    }
                                }
                                this.Discount = totalDiscountPercentage;

                                // Apply the total discount
                                if (totalDiscountPercentage > 0)
                                {
                                    finalAmount *= (1 - (totalDiscountPercentage / 100));
                                }

                                // Create booking with updated amount
                                var booking = new Booking
                                {
                                    CustomerId = customer.Id,
                                    CheckInDate = CheckInDate,
                                    CheckOutDate = CheckOutDate,
                                    Status = "Đang chờ",
                                    TotalAmount = finalAmount.ToString(), // Store the total after applying discount
                                    RoomId = room.Id,
                                    DiscountId = (discountId == 0) ? null : discountId
                                };
                                context.Add(booking);
                            }
                        }
                    }

                    SelectedRooms = selectedRooms;

                    await context.SaveChangesAsync();

                    // Serialize các giá trị cần truyền
                    TempData["SelectedRooms"] = JsonSerializer.Serialize(SelectedRooms);
                    TempData["TotalAmount"] = TotalAmount;
                    TempData["StayDuration"] = StayDuration;
                    TempData["CheckinDate"] = CheckinDate;
                    TempData["CheckoutDate"] = CheckoutDate;

                    return RedirectToPage("/User_/Checkout");
                }
            }

            return Page();
        }

        private async Task<Discount?> GetDiscountByPromoCode(string promoCode)
        {
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                if (string.IsNullOrEmpty(promoCode))
                {
                    return null;
                }

                return await context.Discounts
                    .FirstOrDefaultAsync(d => d.Name == promoCode
                                              && d.EffectiveDate <= DateTime.Now
                                              && d.ExpirationDate >= DateTime.Now);
            }
        }
    }

}