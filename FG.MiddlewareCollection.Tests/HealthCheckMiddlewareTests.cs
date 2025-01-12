using FG.MiddlewareCollection.Middlewares.Monitoring;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Tests
{
    [TestClass]
    public class HealthCheckMiddlewareTests
    {
        private Mock<RequestDelegate> _mockNext;

        [TestInitialize]
        public void Setup()
        {
            _mockNext = new Mock<RequestDelegate>();
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldReturnHealthy_ForHealthEndpoint()
        {
            // Arrange
            var middleware = new HealthCheckMiddleware(_mockNext.Object);
            var context = new DefaultHttpContext();
            context.Request.Path = "/health";

            using var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.AreEqual(200, context.Response.StatusCode);

            // Read the response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(context.Response.Body).ReadToEnd();
            Assert.AreEqual("Healthy", responseBody);

            // Verify next middleware is not called
            _mockNext.Verify(next => next.Invoke(It.IsAny<HttpContext>()), Times.Never);
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldCallNextMiddleware_ForOtherEndpoints()
        {
            // Arrange
            var middleware = new HealthCheckMiddleware(_mockNext.Object);
            var context = new DefaultHttpContext();
            context.Request.Path = "/not-health";

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            // Verify the next middleware is called
            _mockNext.Verify(next => next.Invoke(context), Times.Once);
        }
    }

}
