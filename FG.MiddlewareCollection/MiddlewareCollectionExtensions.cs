using FG.MiddlewareCollection.Middlewares.Monitoring;
using FG.MiddlewareCollection.Middlewares.Performance;
using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace FG.MiddlewareCollection
{
    public static class MiddlewareCollectionExtensions
    {
        /// <summary>
        /// Adds the AdvancedLoggingMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseAdvancedLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AdvancedLoggingMiddleware>();
        }

        /// <summary>
        /// Adds the ErrorLoggingMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseErrorLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorLoggingMiddleware>();
        }

        /// <summary>
        /// Adds the HealthCheckMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HealthCheckMiddleware>();
        }

        /// <summary>
        /// Adds the ResponseCompressionMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseCompressionMiddleware>();
        }

        /// <summary>
        /// Adds the CachingMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseMemcache(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CachingMiddleware>();
        }

        /// <summary>
        /// Adds the CorsMiddleware to the application's request pipeline with specified options.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="options">The CorsMiddlewareOptions instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseCors(this IApplicationBuilder app, ServiceCollection services, CorsMiddlewareOptions options)
        {
            services.AddOptions<CorsMiddlewareOptions>().Configure(o => o = options);
            return app.UseMiddleware<CorsMiddleware>();
        }

        /// <summary>
        /// Adds the CorsMiddleware to the application's request pipeline with options from configuration.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="configuration">The IConfiguration instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseCors(this IApplicationBuilder app, ServiceCollection services, IConfiguration configuration)
        {
            CorsMiddlewareOptions options = new CorsMiddlewareOptions();
            configuration.GetSection("CorsMiddlewareOptions")?.Bind(options);
            services.AddOptions<CorsMiddlewareOptions>().Configure(o => o = options);
            return app.UseMiddleware<CorsMiddleware>();
        }

        /// <summary>
        /// Adds the CircuitBreakerMiddleware to the application's request pipeline with specified threshold and open time.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="threshold">The threshold for the circuit breaker.</param>
        /// <param name="openTime">The open time for the circuit breaker.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseCircuitBreaker(this IApplicationBuilder app, ServiceCollection services, int threshold, TimeSpan openTime)
        {
            services.AddOptions<CircuitBreakerMiddlewareOptions>().Configure(o => o = new CircuitBreakerMiddlewareOptions { OpenTime = openTime, Threshold = threshold });
            return app.UseMiddleware<CircuitBreakerMiddleware>();
        }

        /// <summary>
        /// Adds the CircuitBreakerMiddleware to the application's request pipeline with specified options.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="middlewareOptions">The CircuitBreakerMiddlewareOptions instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseCircuitBreaker(this IApplicationBuilder app, ServiceCollection services, CircuitBreakerMiddlewareOptions middlewareOptions)
        {
            services.AddOptions<CircuitBreakerMiddlewareOptions>().Configure(o => o = middlewareOptions);
            return app.UseMiddleware<CircuitBreakerMiddleware>();
        }

        /// <summary>
        /// Adds the CircuitBreakerMiddleware to the application's request pipeline with options from configuration.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="configuration">The IConfiguration instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseCircuitBreaker(this IApplicationBuilder app, ServiceCollection services, IConfiguration configuration)
        {
            CircuitBreakerMiddlewareOptions options = new CircuitBreakerMiddlewareOptions();
            configuration.GetSection("CircuitBreakerMiddlewareOptions")?.Bind(options);
            services.AddOptions<CircuitBreakerMiddlewareOptions>().Configure(o => o = options);
            return app.UseMiddleware<CircuitBreakerMiddleware>();
        }

        /// <summary>
        /// Adds the IPBlacklistingMiddleware to the application's request pipeline with specified blacklisted IPs.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="blacklistedIPs">The collection of blacklisted IPs.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseIPBlacklisting(this IApplicationBuilder app, ServiceCollection services, IEnumerable<string> blacklistedIPs)
        {
            services.AddSingleton(new IPBlacklist { BlacklistedIPs = blacklistedIPs });
            return app.UseMiddleware<IPBlacklistingMiddleware>();
        }

        /// <summary>
        /// Adds the IPWhitelistingMiddleware to the application's request pipeline with specified whitelisted IPs.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="whitelistedIPs">The collection of whitelisted IPs.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseIPWhitelisting(this IApplicationBuilder app, ServiceCollection services, IEnumerable<string> whitelistedIPs)
        {
            services.AddSingleton(new IPWhitelist { WhitelistedIPs = whitelistedIPs });
            return app.UseMiddleware<IPWhitelistingMiddleware>();
        }

        /// <summary>
        /// Adds the RateLimitingMiddleware to the application's request pipeline with options from configuration.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="configuration">The IConfiguration instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, ServiceCollection services, IConfiguration configuration)
        {
            RateLimitingMiddlewareOptions options = new RateLimitingMiddlewareOptions();
            configuration.GetSection("RateLimitingMiddlewareOptions")?.Bind(options);
            services.AddOptions<RateLimitingMiddlewareOptions>().Configure(o => o = options);
            return app.UseMiddleware<RateLimitingMiddleware>();
        }

        /// <summary>
        /// Adds the RateLimitingMiddleware to the application's request pipeline with specified options.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="options">The RateLimitingMiddlewareOptions instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, ServiceCollection services, RateLimitingMiddlewareOptions options = null)
        {
            services.AddOptions<RateLimitingMiddlewareOptions>().Configure(o => o = options ?? new RateLimitingMiddlewareOptions());
            return app.UseMiddleware<RateLimitingMiddleware>();
        }

        /// <summary>
        /// Adds the RateLimitingMiddleware to the application's request pipeline with specified rate limit and error message.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="services">The ServiceCollection instance.</param>
        /// <param name="rateLimitPerMinute">The rate limit per minute.</param>
        /// <param name="limitExceedErrorMessage">The error message when the rate limit is exceeded.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, ServiceCollection services, int rateLimitPerMinute = 100, string limitExceedErrorMessage = "Rate limit exceeded. Try again later.")
        {
            services.AddOptions<RateLimitingMiddlewareOptions>().Configure(o => o = new RateLimitingMiddlewareOptions
            {
                RequestsPerMinute = rateLimitPerMinute,
                LimitExceedErrorMessage = limitExceedErrorMessage
            });
            return app.UseMiddleware<RateLimitingMiddleware>();
        }

        /// <summary>
        /// Adds the XssProtectionMiddleware to the application's request pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder UseXssProtection(this IApplicationBuilder app)
        {
            return app.UseMiddleware<XssProtectionMiddleware>();
        }
    }
}
