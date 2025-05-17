using BlockArena.Common.Models;
using System.Threading.Tasks;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingHandler
    {
        public Task<Rating> GetRating();
    }
}
