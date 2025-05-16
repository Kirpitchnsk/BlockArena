using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArena.Models;

namespace BlockArena.Interfaces
{
    public interface IScorePipeline
    {
        Task<List<Models.Score>> GetScores(int count);
        Task Add(Score userScore);
    }
}
