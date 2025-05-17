using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlockArena.Middlewares
{
    public class ReverseProxyHttpsRedirect(RequestDelegate next)
    {
        private const string ForwardedProtoHeader = "X-Forwarded-Proto";
        private readonly RequestDelegate next = next;

        public async Task Invoke(HttpContext context)
        {
            var forwardedProto = context.Request.Headers[ForwardedProtoHeader].ToString();

            if (forwardedProto == string.Empty || forwardedProto == "https")
            {
                await next(context);
            }
            else if (forwardedProto != "https")
            {
                var withHttps = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                context.Response.Redirect(withHttps);
            }
        }
    }
}