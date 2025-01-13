using FG.MiddlewareCollection.Middlewares.Monitoring;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FG.MiddlewareCollection.Tests.UnitTests
{
    [TestClass]
    public class ErrorLoggingMiddlewareTests
    {
        private Mock<ILogger<ErrorLoggingMiddleware>> _mockLogger;
        private RequestDelegate _mockNext;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ErrorLoggingMiddleware>>();
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var exceptionToThrow = new InvalidOperationException("Test exception");
            _mockNext = (context) => throw exceptionToThrow;

            var middleware = new ErrorLoggingMiddleware(_mockNext, _mockLogger.Object);
            var context = new DefaultHttpContext();

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));

            // Verify the exception is rethrown
            Assert.AreEqual(exceptionToThrow, exception);

            // Verify the logger captured the exception
            _mockLogger.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An unhandled exception has occurred while executing the request.")),
                    exceptionToThrow,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once);
        }

        [TestMethod]
        public async Task InvokeAsync_ShouldNotLogError_WhenNoExceptionIsThrown()
        {
            // Arrange
            _mockNext = (context) => Task.CompletedTask;

            var middleware = new ErrorLoggingMiddleware(_mockNext, _mockLogger.Object);
            var context = new DefaultHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert no logging occurred
            _mockLogger.Verify(logger =>
                logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Never);
        }
    }

}
