using System.Threading.Tasks;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingUpdater
    {
        Task Add(UserScore userScore);
    }
}
