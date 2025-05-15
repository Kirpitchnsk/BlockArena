using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Interfaces;
using BlockArena.Models;

namespace BlockArena.Controllers.Api
{
    public class StatsFeedController(IScorePipeline userScoreInteractor) : Controller
    {
        readonly IScorePipeline userScoreInteractor = userScoreInteractor;

        [Route("api/player-scores")]
        [HttpHead]
        public object Headers()
        {
            return null;
        }

        [Route("api/player-scores")]
        [HttpGet]
        public async Task<IEnumerable<UserScore>> GetUserScores()
        {
            return await userScoreInteractor.GetUserScores(20);
        }

        [Route("api/player-scores")]
        [HttpPost]
        public async Task AddUserScore([FromBody] UserScore userScore)
        {
            await userScoreInteractor.Add(userScore);
        }
    }
}
