using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Website.Middlewares
{
    public class NewRelicIgnore(RequestDelegate next, string path)
    {
        private readonly RequestDelegate next = next;
        private readonly string path = path;

        public async Task Invoke(HttpContext ctx)
        {
            if (ctx.Request.Path.Value.Equals(path, System.StringComparison.CurrentCultureIgnoreCase)
                || ctx.Request.Path.Value.StartsWith($"{path}/"))
            {
                NewRelic.Api.Agent.NewRelic.IgnoreTransaction();
                NewRelic.Api.Agent.NewRelic.IgnoreApdex();
            }
            await next(ctx);
        }
    }
}