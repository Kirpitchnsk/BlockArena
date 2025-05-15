using System.Threading.Tasks;
using BlockArena.Domain.Models;

namespace BlockArena.Domain.Interfaces
{
    public interface IRatingStorage
    {
        Task Add(UserScore userScore);
    }
}
