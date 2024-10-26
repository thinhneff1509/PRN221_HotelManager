using Project_Group5.Models;

namespace Project_Group5.Repositoríes.Interfaces
{
    public interface IRoomRepository
    {
        Task<IEnumerable<RoomType>> GetRoomTypesAsync();
        Task<RoomType?> GetRoomTypeByIdAsync(int id);
        Task<int> GetAvailiableRooms(int roomtypeId);

    }
}
