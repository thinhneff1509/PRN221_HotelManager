using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5.Pages.User_.Rooms
{
    public class ListModel : PageModel
    {
        [BindProperty]
        public List<RoomType> RoomTypes { get; set; }
        [BindProperty]
        public List<RoomType> AvailableRooms { get; set; }
        [BindProperty]
        public SelectList RoomTypeSelectList { get; set; }
        [BindProperty]
        public int? Id { get; set; }

        public async Task OnGetAsync(int? Id)
        {
            this.Id = Id;

            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                RoomTypeSelectList = new SelectList(await context.RoomTypes.ToListAsync(), "Id", "Name");
            }
            RoomTypes = await GetAvailableRoom(Id);
            AvailableRooms = await GetOccupiedRoom(Id);
        }

        public async Task<List<RoomType>> GetAvailableRoom(int? Id)
        {
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                var roomTypes = await context.RoomTypes
                    .Include(r => r.Rooms.Where(r => r.Status != "Hết phòng"))
                    .ThenInclude(r => r.ImageRooms)
                    .ToListAsync();

                if (Id.HasValue && Id > 0)
                {
                    roomTypes = roomTypes.Where(r => r.Id == Id).ToList();
                }

                // Filter out booked rooms after fetching
                foreach (var roomType in roomTypes)
                {
                    var availableRooms = new List<Room>();
                    foreach (var room in roomType.Rooms)
                    {
                        if (!(await IsRoomBooked(room.Id, DateTime.Now, DateTime.Now.AddDays(1))))
                        {
                            availableRooms.Add(room);
                        }
                    }
                    roomType.Rooms = availableRooms;
                }

                return roomTypes;
            }
        }

        public async Task<List<RoomType>> GetOccupiedRoom(int? Id)
        {
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                var RoomTypes = await context.RoomTypes
                    .Include(r => r.Rooms.Where(r => r.Status == "Hết phòng"))
                    .ThenInclude(r => r.ImageRooms)
                    .ToListAsync();


                var AvailableRooms = await context.RoomTypes
                    .Include(r => r.Rooms.Where(r => r.Status != "Hết phòng"))
                    .ThenInclude(r => r.ImageRooms)
                    .ToListAsync();

                if (Id.HasValue && Id > 0)
                {
                    RoomTypes = RoomTypes.Where(r => r.Id == Id).ToList();
                    AvailableRooms = AvailableRooms.Where(r => r.Id == Id).ToList();
                }

                // Filter out booked rooms after fetching
                foreach (var roomType in AvailableRooms)
                {
                    var bookedRooms = new List<Room>();
                    foreach (var room in roomType.Rooms)
                    {
                        if ((await IsRoomBooked(room.Id, DateTime.Now, DateTime.Now.AddDays(1))))
                        {
                            bookedRooms.Add(room);
                        }
                    }
                    roomType.Rooms = bookedRooms;
                }

                var combinedRoomTypes = new List<RoomType>();
                combinedRoomTypes.AddRange(RoomTypes);
                combinedRoomTypes.AddRange(AvailableRooms);

                return combinedRoomTypes;
            }
        }

        private async Task<bool> IsRoomBooked(int roomId, DateTime checkIn, DateTime checkOut)
        {
            using (Fall24_SE1745_PRN221_Group5Context context = new Fall24_SE1745_PRN221_Group5Context())
            {
                return await context.Bookings
                    .AnyAsync(b => b.RoomId == roomId &&
                                   ((b.CheckInDate <= checkIn && b.CheckOutDate > checkIn) ||
                                    (b.CheckInDate < checkOut && b.CheckOutDate >= checkOut) ||
                                    (b.CheckInDate >= checkIn && b.CheckOutDate <= checkOut)));
            }
        }
    }
}
