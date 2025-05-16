using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Common.Models;
using BlockArena.Common.Interfaces;

namespace BlockArena.Controllers.Api
{
    public class GameRoomController(IRoomStorage roomStorage) : Controller
    {
        private readonly IRoomStorage roomStorage = roomStorage;

        [HttpGet]
        [Route("api/rooms")]
        public async Task<Page<Room>> GetRooms([FromQuery] int start, [FromQuery] int count)
        {
            return await roomStorage.GetRooms(start, count);
        }
    }
}