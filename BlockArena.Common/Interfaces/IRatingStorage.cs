using System.Threading.Tasks;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingStorage
    {
        Task Add(UserScore userScore);
    }
}
