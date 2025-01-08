namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class RateLimitingMiddlewareOptions
    {
        public int RequestsPerMinute { get; set; } = 100;
        public string LimitExceedErrorMessage { get; set; } = "Rate limit exceeded. Try again later.";
    }
}