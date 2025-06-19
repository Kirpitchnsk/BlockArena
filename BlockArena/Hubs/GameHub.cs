using BlockArena.Common;
using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockArena.Hubs
{
    public class GameHub(ILogger<GameHub> logger, IRoomStorage roomStorage) : Hub
    {
        private readonly ILogger<GameHub> logger = logger;
        private readonly IRoomStorage roomStorage = roomStorage;

        [Transaction(Web = true)]
        public async Task Hello(GroupMessage helloMessage)
        {
            var groupId = helloMessage.GroupId;
            var userId = helloMessage.Message.GetProperty("userId").GetString();
            var isRunning = helloMessage.Message.GetProperty("isRunning").GetBoolean();
            helloMessage.Message.TryGetProperty("name", out var name);
            var isOrganizer = userId == groupId;

            Context.Items["userId"] = userId;
            Context.Items["groupId"] = groupId;
            Context.Items["name"] = name;

            await Task.WhenAll(
                Groups.AddToGroupAsync(Context.ConnectionId, groupId),
                Groups.AddToGroupAsync(Context.ConnectionId, isOrganizer ? $"{groupId}-organizer" : $"{groupId}-players")
            );

            if (isOrganizer)
            {
                var reset = isRunning ? Task.FromResult(0) : Clients.Group(groupId).SendAsync("reset");

                var addRoom = roomStorage.AddRoom(new Room
                {
                    OrganizerId = groupId,
                    Status = RoomStatus.Waiting,
                    Players = new Dictionary<string, UserScore>
                    {
                        { groupId, new UserScore { } }
                    }
                });

                await reset;
                await addRoom;
            }
            else
            {
                await Clients.Group($"{groupId}-organizer").SendAsync("addToChat", new
                {
                    notification = "connected",
                    userId
                });

                await Clients.Group($"{groupId}-organizer").SendAsync("hello", helloMessage.Message);
            }
        }

        [Transaction(Web = true)]
        public async Task PlayersListUpdate(GroupMessage playersListUpdateMessage)
        {
            var sendBroadcast = Clients
                .Group($"{playersListUpdateMessage.GroupId}-players")
                .SendAsync("playersListUpdate", playersListUpdateMessage.Message);

            var playersList = playersListUpdateMessage.Message.ConvertTo<PlayersList>();
            var jsonPatch = new JsonPatchDocument<Room>();

            jsonPatch.Replace(x => x.Players, playersList
                .Players
                .Where(player => !player.Disconnected)
                .ToDictionary(x => x.UserId, player => new UserScore
                {
                    Username = player.Name
                }));

            var updateRoom = roomStorage.TryUpdateRoom(jsonPatch, Context.Items["groupId"] as string);

            await sendBroadcast;
            await updateRoom;
        }

        [Transaction(Web = true)]
        public async Task Status(GroupMessage statusMessage)
        {
            var messageHasName = statusMessage
                .Message
                .EnumerateObject()
                .Any(prop => prop.Name == "name" && prop.Value.ValueKind == JsonValueKind.String);

            var nameIsChanging = Context.Items["name"] as string != GetNameFrom(statusMessage);
            var isNameChange = messageHasName && nameIsChanging;

            Task doNameChange()
            {
                var newName = statusMessage.Message.GetProperty("name").GetString();

                if (newName.Length > Common.Constants.MaxUsernameChars)
                {
                    throw new HubException($"Имя должно быть из {Common.Constants.MaxUsernameChars} символов или меньше.");
                }

                Context.Items["name"] = newName;

                var jsonPatch = new JsonPatchDocument<Room>();

                jsonPatch.Replace(
                    x => x.Players[Context.Items["userId"] as string],
                    new UserScore { Username = newName });

                return roomStorage.TryUpdateRoom(jsonPatch, Context.Items["groupId"] as string);
            }

            Task updateDataWhenNameChange()
            {
                return (isNameChange
                    ? Clients.Group(statusMessage.GroupId)
                    : Clients.OthersInGroup(statusMessage.GroupId)).SendAsync("status", statusMessage.Message);
            }

            await Task.WhenAll(
                isNameChange ? doNameChange() : Task.FromResult(0),
                updateDataWhenNameChange()  
            );
        }

        [Transaction(Web = true)]
        public async Task Start(GroupMessage statusMessage)
        {
            var doBroadcast = Clients.Group(statusMessage.GroupId).SendAsync("start");

            var patch = new JsonPatchDocument<Room>();
            patch.Replace(room => room.Status, RoomStatus.Running);

            var updateRoom = roomStorage.TryUpdateRoom(patch, Context.Items["groupId"] as string);

            await doBroadcast;
            await updateRoom;
        }

        [Transaction(Web = true)]
        public async Task Results(GroupMessage resultsMessage)
        {
            var doBroadcast = Clients.Group(resultsMessage.GroupId).SendAsync("results", resultsMessage.Message);

            var jsonPatch = new JsonPatchDocument<Room>();
            jsonPatch.Replace(x => x.Status, RoomStatus.Waiting);

            var updateRoom = roomStorage.TryUpdateRoom(jsonPatch, Context.Items["groupId"] as string);

            await doBroadcast;
            await updateRoom;
        }

        [Transaction(Web = true)]
        public async Task Reset(GroupMessage resetMessage)
        {
            await Clients.Group(resetMessage.GroupId).SendAsync("reset");
        }

        [Transaction(Web = true)]
        public async Task SendChat(GroupMessage chatMessage)
        {
            await Clients.Group(chatMessage.GroupId).SendAsync("addToChat", chatMessage.Message);
        }

        [Transaction(Web = true)]
        public async Task SetChatLines(GroupMessage chatMessage)
        {
            await Clients.OthersInGroup(chatMessage.GroupId).SendAsync("setChatLines", chatMessage.Message);
        }

        [Transaction(Web = true)]
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var groupId = Context.Items["groupId"] as string;
            var userId = Context.Items["userId"] as string;
            var isOrganizer = groupId == userId;

            var disconnect = isOrganizer
                ? Clients.Group(groupId).SendAsync("noOrganizer")
                : Clients.Group(groupId).SendAsync("status", new { userId, disconnected = true });

            await disconnect;

            if (!isOrganizer)
            {
                var name = Context.Items["name"].ToString();

                var doBroadcast = Clients.Group(groupId).SendAsync("addToChat", new
                {
                    notification = "disconnected",
                    userId
                });

                var patch = new JsonPatchDocument<Room>();
                patch.Remove(x => x.Players[userId]);
                var updateRoom = roomStorage.TryUpdateRoom(patch, Context.Items["groupId"] as string);

                await doBroadcast;
                await updateRoom;
            }
            else
            {
                await roomStorage.RemoveRoom(new Room { OrganizerId = groupId });
            }

            if (exception != null)
            {
                logger.LogError(exception, "Disconnected");
            }
        }

        [Transaction(Web = true)]
        public async Task Attack(GroupMessage message)
        {
            await Clients.OthersInGroup(message.GroupId).SendAsync("attack", message.Message);
        }

        public string GetNameFrom(GroupMessage groupMessage)
        {
            var hasKey = groupMessage
                .Message
                .TryGetProperty("name", out var name);

            return hasKey ? name.GetString() : null;
        }
    }
}