using System;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class CircuitBreakerMiddlewareOptions
    {
        public int Threshold { get; set; } = 5;
        public TimeSpan OpenTime { get; set; } = TimeSpan.FromMinutes(1);
    }
}
