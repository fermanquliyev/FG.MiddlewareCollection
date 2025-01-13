using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Tests.UnitTests
{
    [TestClass]
    public class CircuitBreakerMiddlewareTests
    {
        private CircuitBreakerMiddleware middleware;
        private DefaultHttpContext context;
        private CircuitBreakerMiddlewareOptions options;
        private RequestDelegate mockNext;

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            mockNext = new RequestDelegate((context) =>
            {
                if (context.Request.Path.ToString() == "/test-path")
                {
                    throw new Exception("Test exception");
                }
                else
                {
                    return Task.CompletedTask;
                }
            });
            options = new CircuitBreakerMiddlewareOptions
            {
                Threshold = 2,
                OpenTime = TimeSpan.FromSeconds(1)
            };
            middleware = new CircuitBreakerMiddleware(mockNext, Options.Create(options));
            context = new DefaultHttpContext();
        }

        [TestMethod]
        public async Task Should_Break_Circuit()
        {
            context.Request.Path = "/test-path";
            // Act
            foreach (var _ in Enumerable.Range(0, options.Threshold + 1))
            {
                try
                {
                    await middleware.InvokeAsync(context);
                }
                catch (Exception)
                {
                    Console.WriteLine("Exception thrown");
                }
            }
            // Assert
            Assert.AreEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task Should_Not_Break_Circuit()
        {
            context.Request.Path = "/different-path";
            // Act
            foreach (var _ in Enumerable.Range(0, options.Threshold + 1))
            {
                try
                {
                    await middleware.InvokeAsync(context);
                }
                catch (Exception)
                {
                    Console.WriteLine("Exception shouln't be thrown");
                    throw;
                }
            }
            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
        }
    }
}
