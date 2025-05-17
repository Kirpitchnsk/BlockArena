using BlockArena.Common.Models;
using System.Threading.Tasks;

namespace BlockArena.Common.Interfaces
{
    public interface IRatingHandler
    {
        Task<Rating> GetRating();
    }
}
