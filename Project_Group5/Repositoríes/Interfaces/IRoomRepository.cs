using Project_Group5.Models;

namespace Project_Group5.Repositoríes.Interfaces
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
    }
}
