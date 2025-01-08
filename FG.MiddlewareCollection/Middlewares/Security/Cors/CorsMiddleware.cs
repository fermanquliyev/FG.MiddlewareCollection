using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorsMiddlewareOptions _options;

        public CorsMiddleware(RequestDelegate next, IOptions<CorsMiddlewareOptions> options)
        {
            _next = next;
            _options = options.Value ?? new CorsMiddlewareOptions();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", _options.AllowOrigin);
            context.Response.Headers.Add("Access-Control-Allow-Methods", _options.AllowMethods);
            context.Response.Headers.Add("Access-Control-Allow-Headers", _options.AllowHeaders);

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                return;
            }

            await _next(context);
        }
    }
}
