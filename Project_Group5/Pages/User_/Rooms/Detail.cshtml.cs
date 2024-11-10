using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.Rooms
{
    public class DetailModel : PageModel
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public DetailModel(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }


        public RoomType RoomType { get; set; }
        public List<ImageRoom> ImageRoom { get; set; }
        public List<Feedback> Feedbacks { get; set; }
        public int RoomId { get; set; }
        public void OnGet(int id)
        {
            Room Room = _context.Rooms
                           .Include(r => r.Roomtype)
                           .Include(r => r.ImageRooms)
                           .FirstOrDefault(r => r.Id == id);

            if (Room != null)
            {
                RoomType = Room.Roomtype;
                ImageRoom = Room.ImageRooms.ToList();
            }
            else
            {
                // Handle not found case
                RedirectToPage("/NotFound"); // or return NotFound();
            }
            RoomId = id;
        }

        public IActionResult OnPostBookRoom(int roomId)
        {
            Room room = _context.Rooms
                .Include(r => r.Roomtype)
                .Include(r => r.ImageRooms)
                .FirstOrDefault(r => r.Id == roomId);

            if (room == null)
            {
                return RedirectToPage("/NotFound");
            }

            RoomData data = new RoomData
            {
                RoomTypeId = room.RoomtypeId,
                Bed = room.Roomtype.Bed,
                Price = room.Roomtype.Price,
                Name = room.Roomtype.Name,
                RoomList = new List<SelectedRoom>
                {
                    new SelectedRoom
                    {
                        RoomId = room.Id,
                        RoomTypeId = room.RoomtypeId,
                        Name = room.Roomtype.Name,
                        Price = room.Roomtype.Price,
                        AdultCount = 1,
                        ChildrenCount = 0,
                        Bed = room.Roomtype.Bed
                    }
                }
            };

            var serializedRoomData = JsonSerializer.Serialize(data);
            DateTime tomorow = DateTime.Now.AddDays(1);

            // Redirect to the PreOrder page with query parameters
            return RedirectToPage("PreOrder", new
            {
                checkInDate = DateTime.Now.ToString("yyyy-MM-dd"),
                checkOutDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                promoCode = "PromoCode",
                sDiscount = "SDiscount",
                roomData = serializedRoomData
            });        }
    }
}