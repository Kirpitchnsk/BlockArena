using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Website.Middlewares
{
    public class IPLogger(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        private readonly RequestDelegate next = next;
        private readonly ILogger logger = loggerFactory.CreateLogger("IPLogger");

        public async Task Invoke(HttpContext ctx)
        {
            var forwardedIP = ctx.Request.Headers["X-Forwarded-For"].ToString();
            logger.LogInformation("{method} {url} from {ip}{forwardInfo}",
                ctx.Request.Method,
                UriHelper.GetEncodedUrl(ctx.Request),
                ctx.Connection.RemoteIpAddress,
                string.IsNullOrWhiteSpace(forwardedIP) ? "" : $" (forwarded from: {forwardedIP})"
            );
            await next(ctx);
        }
    }
}