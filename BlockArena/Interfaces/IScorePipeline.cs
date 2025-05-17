using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArena.Models;

namespace BlockArena.Interfaces
{
    public interface IScorePipeline
    {
        Task<List<Models.UserScore>> GetScores(int count);
        Task Add(UserScore userScore);
    }
}
