using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class CircuitBreakerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _threshold;
        private readonly TimeSpan _openTime;
        private readonly ConcurrentDictionary<string, (int failureCount, DateTime lastFailureTime, bool circuitOpen)> _pathFailures;

        public CircuitBreakerMiddleware(RequestDelegate next, IOptions<CircuitBreakerMiddlewareOptions> options)
        {
            _next = next;
            var _options = options.Value ?? new CircuitBreakerMiddlewareOptions();
            _threshold = _options.Threshold;
            _openTime = _options.OpenTime;
            _pathFailures = new ConcurrentDictionary<string, (int, DateTime, bool)>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.ToString();
            var (failureCount, lastFailureTime, circuitOpen) = _pathFailures.GetOrAdd(path, (0, DateTime.MinValue, false));

            if (circuitOpen)
            {
                if (DateTime.UtcNow - lastFailureTime > _openTime)
                {
                    _pathFailures[path] = (0, DateTime.MinValue, false);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    return;
                }
            }

            try
            {
                await _next(context);
            }
            catch (Exception)
            {
                failureCount++;
                lastFailureTime = DateTime.UtcNow;

                if (failureCount >= _threshold)
                {
                    circuitOpen = true;
                }

                _pathFailures[path] = (failureCount, lastFailureTime, circuitOpen);
                throw;
            }
        }
    }
}
