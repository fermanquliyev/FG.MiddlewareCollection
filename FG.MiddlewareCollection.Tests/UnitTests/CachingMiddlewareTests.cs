using FG.MiddlewareCollection.Middlewares.Performance;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FG.MiddlewareCollection.Tests.UnitTests
{
    [TestClass]
    public class CachingMiddlewareTests
    {
        private Mock<RequestDelegate> _mockNext;
        private Mock<IMemcache> _mockMemcache;

        [TestInitialize]
        public void Setup()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockMemcache = new Mock<IMemcache>();
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldReturnCachedResponse_WhenCacheExists()
        {
            // Arrange
            var middleware = new CachingMiddleware(_mockNext.Object, _mockMemcache.Object);
            var context = new DefaultHttpContext();
            context.Request.Path = "/test";
            context.Request.QueryString = new QueryString("?key=value");

            // Mock cached response
            var cacheKey = "/test_?key=value";
            _mockMemcache.Setup(m => m.Get<string>(cacheKey)).Returns(new CacheResponse<string>("Cached Response"));

            // Redirect response body to a MemoryStream
            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            // Verify cached response is written
            responseStream.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(responseStream).ReadToEnd();
            Assert.AreEqual("Cached Response", responseBody);

            // Verify next middleware is not called
            _mockNext.Verify(next => next.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldCacheResponse_WhenNoCacheExists()
        {
            // Arrange
            var middleware = new CachingMiddleware(_mockNext.Object, _mockMemcache.Object);
            var context = new DefaultHttpContext();
            context.Request.Path = "/test";
            context.Request.QueryString = new QueryString("?key=value");

            // Mock no cached response
            var cacheKey = "/test_?key=value";
            _mockMemcache.Setup(m => m.Get<string>(cacheKey)).Returns(new CacheResponse<string>());

            // Mock next middleware behavior
            _mockNext.Setup(next => next.Invoke(It.IsAny<HttpContext>())).Callback<HttpContext>(ctx =>
            {
                ctx.Response.WriteAsync("Generated Response").Wait();
            });

            // Redirect response body to a MemoryStream
            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            // Verify response is written
            responseStream.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(responseStream).ReadToEnd();
            Assert.AreEqual("Generated Response", responseBody);

            // Verify response is cached
            _mockMemcache.Verify(m => m.SetAsync(cacheKey, "Generated Response", 60), Times.Once);

            // Verify next middleware is called
            _mockNext.Verify(next => next.Invoke(context), Times.Once);
        }

        [TestMethod]
        public void GenerateCacheKey_ShouldReturnCorrectKey()
        {
            // Arrange
            var middleware = new CachingMiddleware(_mockNext.Object, _mockMemcache.Object);
            var context = new DefaultHttpContext();
            context.Request.Path = "/test";
            context.Request.QueryString = new QueryString("?key=value");

            // Act
            var cacheKey = middleware.GenerateCacheKey(context.Request);

            // Assert
            Assert.AreEqual("/test_?key=value", cacheKey);
        }
    }

}
