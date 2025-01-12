namespace FG.MiddlewareCollection.Middlewares.Performance
{
    public interface IMemcache
    {
        CacheResponse<T> Get<T>(string key);
        bool RemoveAsync(string key);
        void SetAsync<T>(string key, T value, int expirationInSeconds);
    }
}