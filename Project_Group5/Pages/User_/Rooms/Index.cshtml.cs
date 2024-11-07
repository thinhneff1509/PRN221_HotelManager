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
        get => string.IsNullOrEmpty(SelectedRoomsJson) ? new List<SelectedRoom>() : JsonSerializer.Deserialize<List<SelectedRoom>>(SelectedRoomsJson);
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

    public decimal TotalPrice { get; set; }
    [BindProperty]
    public int SelectedRoomCount { get; set; } = 1;

    public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
    }

    public async Task<IActionResult> OnPostChooseRoomAsync(int SelectedRoomId, int SelectedRoomCount)
    {
        RoomTypes = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
        if (RoomTypes == null)
        {
            return NotFound();
        }
        // Ensure SelectedRooms is never null
        var selectedRooms = SelectedRooms ?? new List<SelectedRoom>();

        var roomType = RoomTypes.FirstOrDefault(r => r.Id == SelectedRoomId);
        if (roomType != null)
        {
            // Find all available rooms of the selected type
            var availableRooms = roomType.Rooms
                .Where(r => r.Status.Equals("Còn phòng") && !SelectedRooms.Any(sr => sr.RoomId == r.Id))
                .ToList();

            // Calculate the current count of selected rooms for this room type
            var currentRoomCount = SelectedRooms.Count(r => r.RoomTypeId == SelectedRoomId);
            var roomsToAdd = SelectedRoomCount - currentRoomCount;

            if (roomsToAdd > 0)
            {
                // Add unique available rooms
                for (int i = 0; i < roomsToAdd && i < availableRooms.Count; i++)
                {
                    var newRoom = availableRooms[i];
                    selectedRooms.Add(new SelectedRoom
                    {
                        RoomTypeId = roomType.Id,
                        RoomId = newRoom.Id,
                        Name = newRoom.RoomNumber,
                        RoomType = roomType.Name,
                        Price = roomType.Price
                    });
                }
            }
            else if (roomsToAdd < 0)
            {
                // Remove rooms if SelectedRoomCount is reduced
                for (int i = 0; i < -roomsToAdd; i++)
                {
                    var roomToRemove = selectedRooms.FirstOrDefault(sr => sr.RoomTypeId == SelectedRoomId);
                    if (roomToRemove != null)
                    {
                        selectedRooms.Remove(roomToRemove);
                    }
                }
            }

            // After modifying the list, update the SelectedRoomsJson in TempData
            SelectedRooms = selectedRooms;

            // Recalculate total price after selection change
            CalculateTotalPrice();
        }

        return Page();
    }


    private void CalculateTotalPrice()
    {
        var stayDuration = (CheckOutDate - CheckInDate).Days;
        if (stayDuration <= 0) stayDuration = 1;

        TotalPrice = SelectedRooms.Sum(r => r.Price * stayDuration);

        // Apply promo code discount if applicable
        if (!string.IsNullOrEmpty(PromoCode))
        {
            TotalPrice *= 0.9m; // Example: 10% discount
        }
    }

}

public class SelectedRoom
{
    public int RoomTypeId { get; set; }
    public int RoomId { get; set; }
    public string Name { get; set; }
    public string RoomType { get; set; }
    public decimal Price { get; set; }
}
