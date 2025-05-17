using System.Threading.Tasks;
using BlockArena.Common.Models;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingStorage
    {
        public Task Add(UserScore userScore);
    }
}
