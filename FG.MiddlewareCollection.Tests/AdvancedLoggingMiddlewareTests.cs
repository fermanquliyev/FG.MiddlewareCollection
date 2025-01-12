using FG.MiddlewareCollection.Middlewares.Monitoring;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FG.MiddlewareCollection.Tests
{
    [TestClass]
    public class AdvancedLoggingMiddlewareTests
    {
        private Mock<ILogger<AdvancedLoggingMiddleware>> _mockLogger;
        private RequestDelegate _mockNext;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<AdvancedLoggingMiddleware>>();
            _mockNext = new RequestDelegate((HttpContext context) =>
            {
                context.Response.StatusCode = 200; // Mock the next middleware to set a response status.
                return Task.CompletedTask;
            });
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldLogRequestAndResponseDetails()
        {
            // Arrange
            var middleware = new AdvancedLoggingMiddleware(_mockNext, _mockLogger.Object);
            var context = CreateHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            VerifyLogger(_mockLogger, "Handling request: POST /test-path", "127.0.0.1", "TestUserAgent", "Test request body", "key=value");
            VerifyLogger(_mockLogger, "Handled request: POST /test-path responded with 200");
        }

        private DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/test-path";
            context.Request.QueryString = new QueryString("?key=value");
            context.Request.Headers["User-Agent"] = "TestUserAgent";
            context.Request.Headers["Referer"] = "https://example.com";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            var requestBody = "Test request body";
            var requestBodyBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);
            context.Request.Body = new MemoryStream(requestBodyBytes);
            context.Request.Body.Position = 0;

            return context;
        }

        private void VerifyLogger(Mock<ILogger<AdvancedLoggingMiddleware>> mockLogger, params string[] expectedMessages)
        {
            foreach (var message in expectedMessages)
            {
                mockLogger.Verify(logger =>
                    logger.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(message)),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()
                    ), Times.Once);
            }
        }
    }

}
