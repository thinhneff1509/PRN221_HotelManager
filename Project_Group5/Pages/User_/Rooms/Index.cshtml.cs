using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Project_Group5.Pages.Rooms;

public class IndexModel : PageModel
{
    private readonly Fall24_SE1745_PRN221_Group5Context _context;

    public IList<RoomType> RoomTypes { get; set; }

    [TempData]
    public string SelectedRoomsJson { get; set; }

    public List<SelectedRoom> SelectedRooms
    {
        get => JsonSerializer.Deserialize<List<SelectedRoom>>(SelectedRoomsJson ?? "[]");
        set => SelectedRoomsJson = JsonSerializer.Serialize(value);
    }


    [BindProperty]
    public int SelectedRoomId { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; } = DateTime.Now;

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);

    [BindProperty]
    public string PromoCode { get; set; }
    [BindProperty]
    public decimal Discount { get; set; }
    [BindProperty]
    public decimal TotalPrice { get; set; }
    [BindProperty]
    public int SelectedRoomCount { get; set; } = 1;
    public int AdultCount { get; set; } = 1;
    public int ChildrenCount { get; set; } = 0;
    [BindProperty]
    public List<Discount> SpecialDiscount { get; set; }
    [BindProperty]
    public string SDiscount { get; set; }
    [BindProperty]
    public decimal SDiscountAmount { get; set; }


    public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        RoomTypes = await GetAvailableRoom();
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        this.SpecialDiscount = await GetAllDiscount();
    }

    public async Task<List<Discount>> GetAllDiscount()
    {
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        DateTime today = DateTime.Now.Date;

        return await _context.Discounts
            .Where(d => d.EffectiveDate.HasValue && d.EffectiveDate.Value.Date <= today
                     && d.ExpirationDate.HasValue && d.ExpirationDate.Value.Date >= today)
            .ToListAsync();
    }

    public async Task<List<RoomType>> GetAvailableRoom()
    {
        this.SpecialDiscount = await GetAllDiscount();

        var RoomTypes = await _context.RoomTypes
        .Include(r => r.Rooms.Where(r => r.Status != "Hết phòng"))
        .ThenInclude(r => r.ImageRooms)
        .ToListAsync();

        // Filter out booked rooms after fetching
        foreach (var roomType in RoomTypes)
        {
            var availableRooms = new List<Room>();
            foreach (var room in roomType.Rooms)
            {
                if (!(await IsRoomBooked(room.Id, CheckInDate, CheckOutDate)))
                {
                    availableRooms.Add(room);
                }
            }
            roomType.Rooms = availableRooms;
        }

        return RoomTypes;
    }


    public async Task<IActionResult> OnPostChooseRoomAsync(int SelectedRoomId, int SelectedRoomCount, DateTime CheckInDate, DateTime CheckOutDate, string PromoCode, string SDiscount)
    {
        this.SpecialDiscount = await GetAllDiscount();

        RoomTypes = await GetAvailableRoom();

        var selectedRooms = SelectedRooms;
        var roomType = RoomTypes.FirstOrDefault(r => r.Id == SelectedRoomId);

        if (roomType != null)
        {
            var availableRooms = roomType.Rooms.Where(r => r.Status != "Hết phòng" && !selectedRooms.Any(sr => sr.RoomId == r.Id)).ToList();
            var existingRooms = selectedRooms.Where(sr => sr.RoomTypeId == SelectedRoomId).ToList();
            int roomsToAdd = SelectedRoomCount - existingRooms.Count;

            if (roomsToAdd > 0)
            {
                selectedRooms.AddRange(availableRooms.Take(roomsToAdd).Select(r => new SelectedRoom
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
            else if (roomsToAdd < 0)
            {
                selectedRooms.RemoveAll(sr => sr.RoomTypeId == SelectedRoomId && roomsToAdd++ < 0);
            }
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        SelectedRooms = selectedRooms;
        await CalculatePrice();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeAdultAndChildrenCount(int SelectedRoomId, int AdultCount, int ChildrenCount, DateTime CheckInDate, DateTime CheckOutDate, string PromoCode, string SDiscount)
    {
        this.SpecialDiscount = await GetAllDiscount();

        RoomTypes = await GetAvailableRoom();

        var selectedRoomList = SelectedRooms;
        var selectedRoom = selectedRoomList.FirstOrDefault(r => r.RoomId == SelectedRoomId);
        if (selectedRoom != null)
        {
            selectedRoom.AdultCount = AdultCount;
            selectedRoom.ChildrenCount = ChildrenCount;
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        SelectedRooms = selectedRoomList;
        await CalculatePrice();
        return Page();
    }
    private async Task CalculatePrice()
    {
        this.SpecialDiscount = await GetAllDiscount();
        var selectedRoomList = SelectedRooms;
        var stayDuration = (CheckOutDate - CheckInDate).Days;
        stayDuration = Math.Max(1, stayDuration); // Ensure at least 1 day.

        TotalPrice = 0;

        foreach (var room in selectedRoomList)
        {
            var roomType = RoomTypes.FirstOrDefault(r => r.Id == room.RoomTypeId);

            if (roomType != null)
            {
                int totalGuests = room.AdultCount + room.ChildrenCount;
                decimal roomPrice = room.Price;

                if (totalGuests > roomType.Bed)
                {
                    int extraGuests = totalGuests - roomType.Bed;
                    roomPrice += room.Price * 0.3m * extraGuests; // Increase price by 30% for each extra guest
                }

                TotalPrice += roomPrice * stayDuration;
            }
        }

        decimal totalDiscountPercentage = 0;

        // Apply PromoCode discount if applicable
        if (!string.IsNullOrEmpty(PromoCode))
        {
            var discount = await GetDiscountByPromoCode(PromoCode);
            if (discount != null && decimal.TryParse(discount.Amount, out decimal discountAmount))
            {
                totalDiscountPercentage += discountAmount;
                this.Discount = discountAmount; // Store the PromoCode discount
            }
        }

        // Apply Special discount if applicable
        if (!string.IsNullOrEmpty(SDiscount))
        {
            var discount = await GetDiscountByPromoCode(SDiscount);
            if (discount != null && decimal.TryParse(discount.Amount, out decimal discountAmount))
            {
                totalDiscountPercentage += discountAmount;
                this.SDiscountAmount = discountAmount; // Store the Special discount
            }
        }
    }

    public async Task<IActionResult> OnPostCalculateTotalPrice(DateTime CheckInDate, DateTime CheckOutDate, string PromoCode, string SDiscount, string selectedRoomJson)
    {
        this.SpecialDiscount = await GetAllDiscount();

        RoomTypes = await GetAvailableRoom();
        if (CheckInDate >= CheckOutDate)
        {
            ModelState.AddModelError("CheckInDate", "Check-in date must be before check-out date.");
            return Page();
        }
        if (CheckInDate < DateTime.Today)
        {
            ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
            return Page();
        }
        if (!string.IsNullOrEmpty(selectedRoomJson))
        {
            try
            {
                SelectedRoomsJson = selectedRoomJson;
                var selectedRoomList = JsonSerializer.Deserialize<List<SelectedRoom>>(selectedRoomJson);
                if (selectedRoomList == null || !selectedRoomList.Any())
                {
                    ModelState.AddModelError("", "No rooms selected. Please select at least one room.");
                    return Page();
                }
            }
            catch (JsonException)
            {
                ModelState.AddModelError("", "Invalid room selection data. Please try again.");
                return Page();
            }
        }
        else
        {
            ModelState.AddModelError("", "No rooms selected. Please select at least one room.");
            return Page();
        }
        try
        {
            RoomTypes = await GetAvailableRoom();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error fetching room data. Please try again later.");
            return Page();
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        try
        {
            await CalculatePrice();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error calculating total price. Please try again later.");
            return Page();
        }
        if (!string.IsNullOrEmpty(PromoCode))
        {
            var discount = await GetDiscountByPromoCode(PromoCode);
            if (discount == null)
            {
                ModelState.AddModelError("PromoCode", "Invalid or expired promo code.");
            }
            Discount = Decimal.Parse(discount.Amount);

        }
        if (!string.IsNullOrEmpty(SDiscount))
        {
            var sDiscount = await GetDiscountByPromoCode(SDiscount);
            if (sDiscount == null)
            {
                ModelState.AddModelError("PromoCode", "Invalid or expired promo code.");
            }
            SDiscountAmount = Decimal.Parse(sDiscount.Amount);
        }
        return Page();
    }


    private async Task<bool> IsRoomBooked(int roomId, DateTime checkIn, DateTime checkOut)
    {
        return await _context.Bookings
            .AnyAsync(b => b.RoomId == roomId &&
                           ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                            (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                            (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut)));
    }


    public async Task<IActionResult> OnPostPreOrder(DateTime CheckInDate, DateTime CheckOutDate, string PromoCode, string SDiscount, string selectedRoomJson)
    {
        if (!String.IsNullOrEmpty(selectedRoomJson))
        {
            SelectedRoomsJson = selectedRoomJson;
        }
        List<RoomData> roomDatas = new List<RoomData>();
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
        var selectedRoomList = SelectedRooms;

        // Group selected rooms by RoomTypeId
        var groupedRooms = selectedRoomList.GroupBy(r => r.RoomTypeId);

        // Create a RoomData object for each unique RoomTypeId
        foreach (var group in groupedRooms)
        {
            var roomType = RoomTypes.FirstOrDefault(rt => rt.Id == group.Key);
            if (roomType != null)
            {
                var roomData = new RoomData
                {
                    Id = roomType.Id,
                    Bed = roomType.Bed,
                    Price = roomType.Price,
                    Name = roomType.Name,
                    RoomList = group.ToList()
                };
                roomDatas.Add(roomData);
            }
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        this.PromoCode = PromoCode;
        this.SDiscount = SDiscount;
        SelectedRooms = selectedRoomList;
        // Store data in TempData for the redirect
        var serializedRoomData = JsonSerializer.Serialize(roomDatas);
        return RedirectToPage("PreOrder", new { CheckInDate, CheckOutDate, PromoCode, SDiscount, roomData = serializedRoomData });
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


