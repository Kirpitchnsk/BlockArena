using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;

namespace BlockArena.Controllers.Api
{
    public class GameRoomController(IGameRoomStorage roomStorage) : Controller
    {
        private readonly IGameRoomStorage roomStorage = roomStorage;

        [HttpGet]
        [Route("api/rooms")]
        public async Task<Page<Room>> GetGameRooms([FromQuery] int start, [FromQuery] int count)
        {
            return await roomStorage.GetGameRooms(start, count);
        }
    }
}