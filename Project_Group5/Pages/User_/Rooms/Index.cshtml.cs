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
    public decimal TotalPrice { get; set; }
    [BindProperty]
    public int SelectedRoomCount { get; set; } = 1;
    public int AdultCount { get; set; } = 1;
    public int ChildrenCount { get; set; } = 0;


    public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
    }

    public async Task<IActionResult> OnPostChooseRoomAsync(int SelectedRoomId, int SelectedRoomCount, DateTime CheckInDate, DateTime CheckOutDate)
    {
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();

        var selectedRooms = SelectedRooms;
        var roomType = RoomTypes.FirstOrDefault(r => r.Id == SelectedRoomId);

        if (roomType != null)
        {
            var availableRooms = roomType.Rooms.Where(r => r.Status == "Còn phòng" && !selectedRooms.Any(sr => sr.RoomId == r.Id)).ToList();
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
        SelectedRooms = selectedRooms;
        CalculatePrice();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeAdultAndChildrenCount(int SelectedRoomId, int AdultCount, int ChildrenCount, DateTime CheckInDate, DateTime CheckOutDate)
    {
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();

        var selectedRoomList = SelectedRooms;
        var selectedRoom = selectedRoomList.FirstOrDefault(r => r.RoomId == SelectedRoomId);
        if (selectedRoom != null)
        {
            selectedRoom.AdultCount = AdultCount;
            selectedRoom.ChildrenCount = ChildrenCount;
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        SelectedRooms = selectedRoomList;
        CalculatePrice();
        return Page();
    }
    private void CalculatePrice()
    {
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

        // Apply promo code discount if applicable
        if (!string.IsNullOrEmpty(PromoCode))
        {
            TotalPrice *= 0.9m; // Example: 10% discount
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        SelectedRooms = selectedRoomList;

    }

    public async Task<IActionResult> OnPostCalculateTotalPrice(DateTime CheckInDate, DateTime CheckOutDate, string PromoCode, string selectedRoomJson)
    {
        if (!String.IsNullOrEmpty(selectedRoomJson))
        {
            SelectedRoomsJson = selectedRoomJson;

        }
        var selectedRoomList = SelectedRooms;

        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
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

        // Apply promo code discount if applicable
        if (!string.IsNullOrEmpty(PromoCode))
        {
            TotalPrice *= 0.9m; // Example: 10% discount
        }
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        SelectedRooms = selectedRoomList;
        // Return the page after recalculating the total price
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

    public async Task<IActionResult> OnPostCheckAvailabilityAsync()
    {
        var selectedRoomList = SelectedRooms;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RoomTypes = await _context.RoomTypes
            .Include(rt => rt.Rooms)
            .ThenInclude(r => r.ImageRooms)
            .ToListAsync();

        foreach (var roomType in RoomTypes)
        {
            var availableRooms = new List<Room>();
            foreach (var room in roomType.Rooms)
            {
                if (!await IsRoomBooked(room.Id, CheckInDate, CheckOutDate))
                {
                    availableRooms.Add(room);
                }
            }
            roomType.Rooms = availableRooms;
        }

        // Remove room types with no available rooms
        RoomTypes = RoomTypes.Where(rt => rt.Rooms.Any()).ToList();
        this.CheckInDate = CheckInDate;
        this.CheckOutDate = CheckOutDate;
        SelectedRooms = selectedRoomList;
        TempData["AvailabilityChecked"] = true;
        return Page();
    }

    public async Task<IActionResult> OnPostPreOrder(DateTime CheckInDate, DateTime CheckOutDate, string selectedRoomJson)
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
                    RoomTypeId = roomType.Id,
                    RoomType = roomType.Name,
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
        SelectedRooms = selectedRoomList;
        // Store data in TempData for the redirect
        var serializedRoomData = JsonSerializer.Serialize(roomDatas);
        return RedirectToPage("PreOrder", new { CheckInDate, CheckOutDate, roomData = serializedRoomData });
    }


}


