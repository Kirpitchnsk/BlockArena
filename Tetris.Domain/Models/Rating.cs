using System.Collections.Generic;

namespace BlockArena.Domain.Models
{
    public class Rating
    {
        public List<UserScore> UserScores { get; set; }
    }
}