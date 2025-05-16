namespace BlockArena.Common.Models
{
    public record Player
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public bool Disconnected { get; set; }
    }
}
