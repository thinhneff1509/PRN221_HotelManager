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

        public async Task<IEnumerable<RoomType>> GetRoomTypesAsync()
        {
            // Include related entities like ImageRoom
            return await _context.RoomTypes.Include(r => r.ImageRooms).Include(r => r.Rooms).ToListAsync();
        }

        public async Task<RoomType?> GetRoomTypeByIdAsync(int id)
        {
            return await _context.RoomTypes
                                 .Include(r => r.ImageRooms).Include(r => r.Rooms)
                                 .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<int> GetAvailiableRooms(int roomtypeId)
        {
            return await _context.Rooms.Where(r => r.RoomtypeId == roomtypeId && r.Status == 0).CountAsync();
        }

    }
}
