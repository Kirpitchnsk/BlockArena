using System.Threading.Tasks;

namespace BlockArena.Domain.Interfaces
{
    public interface IRatingHandler
    {
        Task<Models.Rating> GetRating();
    }
}
