using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Performance
{
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemcache _memcachedClient;

        public CachingMiddleware(RequestDelegate next, IMemcache memcache)
        {
            _next = next;
            _memcachedClient = memcache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cacheKey = GenerateCacheKey(context.Request);
            var cachedResponse = _memcachedClient.Get<string>(cacheKey);

            if (cachedResponse.HasValue)
            {
                await context.Response.WriteAsync(cachedResponse.Value);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                _memcachedClient.SetAsync(cacheKey, responseText, 60); // Cache for 60 seconds
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        public string GenerateCacheKey(HttpRequest request)
        {
            return $"{request.Path}_{request.QueryString}";
        }
    }
}
