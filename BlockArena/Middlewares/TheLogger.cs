using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlockArena.Middlewares
{
    public class TheLogger(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        private readonly RequestDelegate next = next;
        private readonly ILogger logger = loggerFactory.CreateLogger("IPLogger");

        public async Task Invoke(HttpContext ctx)
        {
            var forwardedIP = ctx.Request.Headers["X-Forwarded-For"].ToString();

            logger.LogInformation("{method} {url} from {ip}{forwardInfo}",
                ctx.Request.Method,
                ctx.Request.GetEncodedUrl(),
                ctx.Connection.RemoteIpAddress,
                string.IsNullOrWhiteSpace(forwardedIP) ? "" : $" (forwarded from: {forwardedIP})"
            );

            await next(ctx);
        }
    }
}