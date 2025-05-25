using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlockArena.Interfaces;
using BlockArena.Models;

namespace BlockArena.Controllers.Api
{
    public class StatsFeedController(IScorePipeline scorePipleline) : Controller
    {
        private readonly IScorePipeline scorePipeline = scorePipleline;

        [Route("api/player-scores")]
        [HttpHead]
        public object Headers()
        {
            return null;
        }

        [Route("api/player-scores")]
        [HttpGet]
        public async Task<IEnumerable<UserScore>> GetScores()
        {
            return await scorePipeline.GetScores(5);
        }

        [Route("api/player-scores")]
        [HttpPost]
        public async Task AddUserScore([FromBody] UserScore score)
        {
            await scorePipeline.Add(score);
        }
    }
}
