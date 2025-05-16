using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRoomStorage
    {
        Task AddRoom(Room room);
        Task TryUpdateRoom(JsonPatchDocument<Room> patch, string roomCode);
        Task RemoveRoom(Room gameRoom);
        Task<Page<Room>> GetRooms(int start, int count);
    }
}