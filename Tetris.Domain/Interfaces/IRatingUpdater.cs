using System.Threading.Tasks;
using BlockArena.Domain.Models;

namespace BlockArena.Domain.Interfaces
{
    public interface IRatingUpdater
    {
        Task Add(UserScore userScore);
    }
}
