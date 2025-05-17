using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Interfaces;
using BlockArena.Models;

namespace BlockArena.Controllers.Api
{
    public class StatsFeedController(IScorePipeline scoreInteractor) : Controller
    {
        readonly IScorePipeline scoreInteractor = scoreInteractor;

        [Route("api/player-scores")]
        [HttpHead]
        public object Headers()
        {
            return null;
        }

        [Route("api/player-scores")]
        [HttpGet]
        public async Task<IEnumerable<Score>> GetScores()
        {
            return await scoreInteractor.GetScores(20);
        }

        [Route("api/player-scores")]
        [HttpPost]
        public async Task AddUserScore([FromBody] Score score)
        {
            await scoreInteractor.Add(score);
        }
    }
}
