using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Monitoring
{
    public class AdvancedLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdvancedLoggingMiddleware> _logger;

        public AdvancedLoggingMiddleware(RequestDelegate next, ILogger<AdvancedLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Read the request body as a string
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Log request details
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var referer = context.Request.Headers["Referer"].ToString();
            _logger.LogInformation("Handling request: {Method} {Path} | Querystring: {Querystring} | Body: {Body} | IP: {IP} | UserAgent: {UserAgent} | Referer: {Referer}",
                context.Request.Method, context.Request.Path, context.Request.QueryString.Value, body, ipAddress, userAgent, referer);

            await _next(context);

            // Log response details
            _logger.LogInformation("Handled request: {Method} {Path} responded with {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);

            // Log performance metrics
            stopwatch.Stop();
            _logger.LogInformation("Request processing time: {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
