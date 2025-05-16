using System;
using System.Collections.Generic;

namespace BlockArena.Common.Models
{
    public record Room
    {
        public Dictionary<string, UserScore> Players { get; set; }
        public RoomStatus Status { get; set; }
        public string OrganizerId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}