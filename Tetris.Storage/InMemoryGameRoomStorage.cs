using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;

namespace BlockArena.Database
{
    public class InMemoryGameRoomStorage : IGameRoomStorage
    {
        private List<Room> roomStorage = new List<Room>();

        public Task AddGameRoom(Room gameRoom)
        {
            if (!roomStorage.Any(room => room.OrganizerId == gameRoom.OrganizerId))
            {
                roomStorage.Add(gameRoom);
            }

            return Task.FromResult(0);
        }

        public Task TryUpdateGameRoom(JsonPatchDocument<Room> patch, string gameRoomCode)
        {
            try
            {
                var theGameRoom = roomStorage.FirstOrDefault(room => room.OrganizerId == gameRoomCode);

                if (theGameRoom != null)
                {
                    patch.ApplyTo(theGameRoom);
                }
            }
            catch (Exception) 
            { 

            }

            return Task.FromResult(0);
        }

        public Task<Page<Room>> GetGameRooms(int start, int count)
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

        public Task RemoveGameRoom(Room room)
        {
            roomStorage = roomStorage.Where(x => x.OrganizerId != room.OrganizerId).ToList();
            return Task.FromResult(0);
        }
    }
}