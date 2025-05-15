using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArena.Models;

namespace BlockArena.Interfaces
{
    public interface IScorePipeline
    {
        Task<List<Models.UserScore>> GetUserScores(int count);
        Task Add(UserScore userScore);
    }
}
