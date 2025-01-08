using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitingMiddlewareOptions options;
        private static readonly ConcurrentDictionary<string, (DateTime Timestamp, int Count)> _clients = new ConcurrentDictionary<string, (DateTime, int)>();

        public RateLimitingMiddleware(RequestDelegate next, IOptions<RateLimitingMiddlewareOptions> options)
        {
            _next = next;
            this.options = options.Value ?? new RateLimitingMiddlewareOptions
            {
                RequestsPerMinute = 100,
                LimitExceedErrorMessage = "Rate limit exceeded. Try again later."
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (clientIp == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Unable to determine client IP.");
                return;
            }

            var now = DateTime.UtcNow;
            var clientData = _clients.GetOrAdd(clientIp, (now, 0));

            if (clientData.Timestamp.AddMinutes(1) < now)
            {
                clientData = (now, 0);
            }

            if (clientData.Count >= options.RequestsPerMinute)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync(options.LimitExceedErrorMessage);
                return;
            }

            _clients[clientIp] = (clientData.Timestamp, clientData.Count + 1);
            await _next(context);
        }
    }
}
