using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Common.Models;
using BlockArena.Common.Interfaces;
using System;

namespace BlockArena.Controllers.Api
{
    public class GameRoomController(IRoomStorage roomStorage) : Controller
    {
        private readonly IRoomStorage roomStorage = roomStorage;

        [HttpGet]
        [Route("api/rooms")]
        public async Task<IActionResult> GetRooms([FromQuery] int start, [FromQuery] int count)
        {
            try
            {
                var page = await roomStorage.GetRooms(start, count);
                return Ok(page);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Mongo ERROR] {ex.GetType().Name}: {ex.Message}");
                return StatusCode(500, $"MongoDB error: {ex.Message}");
            }
        }
    }
}