using System.Collections.Generic;

namespace BlockArena.Domain.Models
{
    public record PlayersList
    {
        public List<Player> Players { get; set; }
    }
}