using System;
using System.Collections.Generic;

namespace BlockArena.Domain.Models
{
    public record Room
    {
        public Dictionary<string, UserScore> Players { get; set; }
        public GameRoomStatus Status { get; set; }
        public string OrganizerId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}