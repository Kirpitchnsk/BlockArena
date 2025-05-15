using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using BlockArena.Domain.Models;

namespace BlockArena.Domain.Interfaces
{
    public interface IGameRoomStorage
    {
        Task AddGameRoom(Room gameRoom);
        Task TryUpdateGameRoom(JsonPatchDocument<Room> patch, string gameRoomCode);
        Task RemoveGameRoom(Room gameRoom);
        Task<Page<Room>> GetGameRooms(int start, int count);
    }
}