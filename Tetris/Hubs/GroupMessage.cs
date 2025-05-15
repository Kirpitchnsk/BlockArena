using System.Text.Json;

namespace BlockArena.Hubs
{
    public class GroupMessage
    {
        public string GroupId { get; set; }
        public JsonElement Message { get; set; }
    }
}