using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using System.ComponentModel.DataAnnotations;

namespace Project_Group5.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public IList<RoomType> Rooms { get; set; }

        [BindProperty]
        public List<SelectedRoom> SelectedRooms { get; set; } = new();

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

        public IndexModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Rooms = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();
        }

        public async Task<IActionResult> OnPostChooseRoomAsync(int roomId)
        {
            Rooms = await _context.RoomTypes.Include(r => r.Rooms).ThenInclude(r => r.ImageRooms).ToListAsync();

            var room = Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room != null)
            {
                SelectedRooms.Add(new SelectedRoom { RoomId = room.Id, RoomName = room.Name, Price = room.Price });
                CalculateTotalPrice();
            }

            return Page();
        }

        private void CalculateTotalPrice()
        {
            var stayDuration = (CheckOutDate - CheckInDate).Days;
            if (stayDuration <= 0) stayDuration = 1;

            TotalPrice = SelectedRooms.Sum(r => r.Price * stayDuration);
            if (!string.IsNullOrEmpty(PromoCode))
            {
                TotalPrice *= 0.9m; // Giảm giá 10% ví dụ
            }
        }
    }

    public class SelectedRoom
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public decimal Price { get; set; }
    }
}
