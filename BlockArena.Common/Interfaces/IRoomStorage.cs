using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRoomStorage
    {
        public Task AddRoom(Room room);
        public Task TryUpdateRoom(JsonPatchDocument<Room> jsonPatch, string roomCode);
        public Task RemoveRoom(Room room);
        public Task<Page<Room>> GetRooms(int start, int count);
    }
}