using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using BlockArena.Common.Models;
using BlockArena.Common.Interfaces;

namespace BlockArena.Database
{
    public class InMemoryGameRoomStorage : IRoomStorage
    {
        private List<Room> roomStorage = new List<Room>();

        public Task AddRoom(Room room)
        {
            if (!roomStorage.Any(room => room.OrganizerId == room.OrganizerId))
            {
                roomStorage.Add(room);
            }

            return Task.FromResult(0);
        }

        public Task TryUpdateRoom(JsonPatchDocument<Room> jsonPatch, string roomCode)
        {
            try
            {
                var room = roomStorage.FirstOrDefault(x => x.OrganizerId == roomCode);

                if (room != null)
                {
                    jsonPatch.ApplyTo(room);
                }
            }
            catch (Exception) 
            { 

            }

            return Task.FromResult(0);
        }

        public Task<Page<Room>> GetRooms(int start, int count)
        {
            var gameRooms = roomStorage.Skip(start).Take(count).ToList();

            return Task.FromResult(new Page<Room>
            {
                Items = gameRooms,
                Total = roomStorage.Count,
                Start = start + count > roomStorage.Count
                    ? roomStorage.Count - gameRooms.Count
                    : start,
                Count = gameRooms.Count
            });
        }

        public Task RemoveRoom(Room room)
        {
            roomStorage = roomStorage.Where(x => x.OrganizerId != room.OrganizerId).ToList();
            return Task.FromResult(0);
        }
    }
}