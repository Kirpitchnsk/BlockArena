using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Driver;
using BlockArena.Common.Models;
using BlockArena.Common.Interfaces;

namespace BlockArena.Database
{
    public class MongoRoomStorage(IMongoClient client) : IRoomStorage
    {
        private readonly IMongoCollection<Room> roomsCollection = client.GetDatabase(DbName).GetCollection<Room>(CollectionName);
        private const string DbName = "tetris";
        private const string CollectionName = "rooms";

        public async Task AddRoom(Room room)
        {
            room.Timestamp = DateTime.UtcNow;

            var filterRooms = Builders<Room>.Filter.Eq(x => x.OrganizerId, room.OrganizerId);
            var replaceOptions = new ReplaceOptions 
            { 
                IsUpsert = true 
            };

            await roomsCollection.ReplaceOneAsync(filterRooms, room, replaceOptions);
        }

        public async Task TryUpdateRoom(JsonPatchDocument<Room> jsonPatch, string roomCode)
        {
            try
            {
                jsonPatch.Replace(room => room.Timestamp, DateTime.UtcNow);
                var filter = Builders<Room>.Filter.Eq(x => x.OrganizerId, roomCode);

                await roomsCollection.UpdateOneAsync(filter, jsonPatch.ToMongoUpdate());
            }
            catch (Exception ex)
            {

            }
        }

        public async Task RemoveRoom(Room room)
        {
            var filter = Builders<Room>.Filter.Eq(x => x.OrganizerId, room.OrganizerId);
            await roomsCollection.DeleteOneAsync(filter);
        }

        public async Task<Page<Room>> GetRooms(int start, int count)
        {
            var countRooms = await roomsCollection.CountDocumentsAsync(Builders<Room>.Filter.Empty);

            if (start + count > countRooms)
            {
                var totalPages = (long)Math.Ceiling(countRooms / (double)count) - 1;
                start = Math.Max((int)(totalPages * count), 0);
            }

            var getRooms = roomsCollection.Find(Builders<Room>.Filter.Empty)
                .Project<Room>(Builders<Room>.Projection.Exclude("_id"))
                .Skip(start)
                .Limit(count)
                .ToListAsync();

            var rooms = await getRooms;

            var page = new Page<Room>
            {
                Total = (int)countRooms,
                Start = start,
                Count = rooms.Count,
                Items = rooms
            };

            return page;
        }
    }
}
