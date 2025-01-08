using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Performance
{
    public class Memcache
    {
        private static readonly ConcurrentDictionary<string, (object Value, DateTime Expiration)> _cache = new ConcurrentDictionary<string, (object, DateTime)>();

        public CacheResponse<T> Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var cacheEntry))
            {
                if(cacheEntry.Expiration > DateTime.UtcNow)
                return new CacheResponse<T> { HasValue = true, Value = (T)cacheEntry.Value };
                else
                {
                    _cache.TryRemove(key, out _);
                }
            }
            return new CacheResponse<T> { HasValue = false };
        }

        public void SetAsync<T>(string key, T value, int expirationInSeconds)
        {
            var expiration = DateTime.UtcNow.AddSeconds(expirationInSeconds);
            _cache[key] = (value, expiration);
        }

        public bool RemoveAsync(string key)
        {
            return _cache.TryRemove(key, out _);
        }
    }

    public class CacheResponse<T>
    {
        public bool HasValue { get; set; }
        public T Value { get; set; }
    }
}
