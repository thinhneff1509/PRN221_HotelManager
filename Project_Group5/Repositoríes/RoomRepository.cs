using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using Project_Group5.Repositoríes.Interfaces;

namespace Project_Group5.Repositoríes
{
    public class RoomRepository : IRoomRepository
    {
        private readonly Fall24_SE1745_PRN221_Group5Context _context;

        public RoomRepository(Fall24_SE1745_PRN221_Group5Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetRoomsAsync()
        {
            // Include related entities like ImageRoom
            return await _context.Rooms.Include(r => r.ImageRooms).ToListAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms
                                 .Include(r => r.ImageRooms)
                                 .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
