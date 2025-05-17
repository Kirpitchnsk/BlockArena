using System.Threading.Tasks;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingHandler
    {
        Task Add(UserScore userScore);
    }
}
