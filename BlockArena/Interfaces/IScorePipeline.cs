using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArena.Models;

namespace BlockArena.Interfaces
{
    public interface IScorePipeline
    {
        public Task<List<UserScore>> GetScores(int count);
        public Task Add(UserScore userScore);
    }
}
