namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class CorsMiddlewareOptions
    {
        public string AllowOrigin { get; set; } = "*";
        public string AllowMethods { get; set; } = "GET, POST, PUT, DELETE, OPTIONS";
        public string AllowHeaders { get; set; } = "Content-Type, Authorization";
    }
}
