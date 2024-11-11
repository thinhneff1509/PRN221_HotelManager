using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Customer")]
    public class PreOrderModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public PreOrderModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

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

            var selectedRooms = SelectedRooms;
            var roomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();

            foreach (var r in selectedRooms)
            {
                r.AvailableRoom = _context.Rooms
                    .Where(rl => rl.Status != "Hết phòng" && r.Id == rl.RoomtypeId).Count();
            }

            decimal totalDiscountPercentage = 0;

            if (!string.IsNullOrEmpty(PromoCode))
            {
                var discount = await GetDiscountByPromoCode(PromoCode);
                if (discount != null &&
                    decimal.TryParse(discount.Amount, out decimal discountAmount))
                {
                    totalDiscountPercentage += discountAmount;
                }
            }

            if (!string.IsNullOrEmpty(SDiscount))
            {
                var discount = await GetDiscountByPromoCode(SDiscount);
                if (discount != null &&
                    decimal.TryParse(discount.Amount, out decimal discountAmount))
                {
                    totalDiscountPercentage += discountAmount;
                }
            }
            Discount = totalDiscountPercentage;

            SelectedRooms = selectedRooms;
            CheckinDate = CheckinDate;
            CheckoutDate = CheckoutDate;
            StayDuration = StayDuration;
            return Page();
        }

        public async Task<IActionResult> OnPostChangeBookingInfo(int roomId, int roomTypeId, int AdultCount,
            int ChildrenCount, DateTime checkinDate, DateTime checkoutDate, Decimal Discount, string PromoCode, int SDiscount)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            StayDuration = StayDuration;
            var selectedRooms = SelectedRooms;
            if (SelectedRooms == null)
            {
                return Page();
            }

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

            this.Discount = Discount;
            SelectedRooms = selectedRooms;
            return Page();
        }

        public async Task<IActionResult> OnPostChangeRoomNum(int roomTypeId, int roomCount, DateTime checkinDate,
            DateTime checkoutDate, Decimal Discount, string PromoCode, int SDiscount)
        {
            CheckinDate = checkinDate;
            CheckoutDate = checkoutDate;
            StayDuration = StayDuration;
            var selectedRooms = SelectedRooms;

            if (selectedRooms == null)
            {
                return BadRequest("Invalid room data.");
            }

            var roomType = await _context.RoomTypes
                .Include(r => r.Rooms.Where(r => r.Status != "Hết phòng"))
                .ThenInclude(r => r.ImageRooms)
                .FirstOrDefaultAsync(r => r.Id == roomTypeId);

            if (roomType == null)
            {
                return NotFound($"RoomType with ID {roomTypeId} not found.");
            }

            var roomGroup = selectedRooms.FirstOrDefault(r => r.Id == roomTypeId);

            if (roomGroup == null)
            {
                return BadRequest("Room group data not found.");
            }

            int numberOfRoomChange = roomCount - roomGroup.RoomList.Count;

            if (numberOfRoomChange > 0)
            {
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
                roomGroup.RoomList.RemoveAll(sr => sr.RoomTypeId == roomTypeId && numberOfRoomChange++ < 0);
            }

            this.Discount = Discount;
            SelectedRooms = selectedRooms;
            return Page();
        }

        public async Task<IActionResult> OnPostPreOrderAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var customer = await _context.Customers.FindAsync(int.Parse(userId));
                if (customer != null)
                {
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
                var selectedRooms = SelectedRooms;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email.Equals(Email) || c.Phone.Equals(Phone));

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
                    await _context.Customers.AddAsync(customer);
                    await _context.SaveChangesAsync();
                }

                DateTime CheckInDate = CheckinDate;
                DateTime CheckOutDate = CheckoutDate;
                int discountId = _context.Discounts.FirstOrDefault(d => d.Name == PromoCode || d.Name == SDiscount)?.Id ?? 0;

                foreach (var r in selectedRooms)
                {
                    foreach (var selectedRoom in r.RoomList)
                    {
                        var room = await _context.Rooms.FirstOrDefaultAsync(ri => ri.Id == selectedRoom.RoomId);
                        if (room != null)
                        {
                            room.Status = "Đang đặt cọc";
                            _context.Update(room);

                            Decimal finalAmount = Decimal.Parse(TotalAmount);
                            decimal totalDiscountPercentage = 0;

                            if (!string.IsNullOrEmpty(PromoCode))
                            {
                                var discount = await GetDiscountByPromoCode(PromoCode);
                                if (discount != null && decimal.TryParse(discount.Amount, out decimal discountAmount))
                                {
                                    totalDiscountPercentage += discountAmount;
                                }
                            }

                            if (!string.IsNullOrEmpty(SDiscount))
                            {
                                var discount = await GetDiscountByPromoCode(SDiscount);
                                if (discount != null && decimal.TryParse(discount.Amount, out decimal discountAmount))
                                {
                                    totalDiscountPercentage += discountAmount;
                                }
                            }
                            Discount = totalDiscountPercentage;

                            if (totalDiscountPercentage > 0)
                            {
                                finalAmount *= (1 - (totalDiscountPercentage / 100));
                            }

                            var booking = new Booking
                            {
                                CustomerId = customer.Id,
                                CheckInDate = CheckInDate,
                                CheckOutDate = CheckOutDate,
                                Status = "Chờ thanh toán",
                                TotalAmount = finalAmount.ToString(),
                                RoomId = room.Id,
                                DiscountId = discountId == 0 ? null : discountId
                            };
                            _context.Add(booking);
                        }
                    }
                }

                SelectedRooms = selectedRooms;
                await _context.SaveChangesAsync();

                TempData["SelectedRooms"] = JsonSerializer.Serialize(SelectedRooms);
                TempData["TotalAmount"] = TotalAmount;
                TempData["StayDuration"] = StayDuration;
                TempData["CheckinDate"] = CheckinDate;
                TempData["CheckoutDate"] = CheckoutDate;

                return RedirectToPage("/User_/Checkout/Checkout");
            }

            return Page();
        }

        private async Task<Discount?> GetDiscountByPromoCode(string promoCode)
        {
            if (string.IsNullOrEmpty(promoCode))
            {
                return null;
            }

            return await _context.Discounts
                .FirstOrDefaultAsync(d => d.Name == promoCode
                                          && d.EffectiveDate <= DateTime.Now
                                          && d.ExpirationDate >= DateTime.Now);
        }
    }
}
