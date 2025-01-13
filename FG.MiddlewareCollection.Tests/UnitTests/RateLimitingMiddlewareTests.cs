using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class RateLimitingMiddlewareUnitTests
{
    [TestMethod]
    public async Task ShouldAllowRequestsBelowRateLimit()
    {
        // Arrange
        var options = Options.Create(new RateLimitingMiddlewareOptions { RequestsPerMinute = 5 });
        var middleware = CreateMiddleware(options, out var context, out var mockNext);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        for (int i = 0; i < 5; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Requests should be successful
        mockNext.Verify(next => next(context), Times.Exactly(5)); // Middleware should allow all 5 requests
    }

    [TestMethod]
    public async Task ShouldBlockRequestsExceedingRateLimit()
    {
        // Arrange
        var options = Options.Create(new RateLimitingMiddlewareOptions { RequestsPerMinute = 3 });
        var middleware = CreateMiddleware(options, out var context, out _);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        for (int i = 0; i < 3; i++)
        {
            await middleware.InvokeAsync(context);
        }
        Assert.AreEqual(200, context.Response.StatusCode);

        // Reset the response for the next request
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context); // Exceed rate limit

        // Assert
        Assert.AreEqual(429, context.Response.StatusCode); // Too Many Requests
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseMessage = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.AreEqual("Rate limit exceeded. Try again later.", responseMessage); // Default error message
    }

    [TestMethod]
    public async Task ShouldReturnBadRequest_WhenClientIPCannotBeDetermined()
    {
        // Arrange
        var options = Options.Create(new RateLimitingMiddlewareOptions());
        var middleware = CreateMiddleware(options, out var context, out _);

        context.Connection.RemoteIpAddress = null; // Simulate missing IP

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(400, context.Response.StatusCode); // Bad Request
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseMessage = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.AreEqual("Unable to determine client IP.", responseMessage);
    }

    [TestMethod]
    public async Task ShouldResetRateLimitAfterOneMinute()
    {
        // Arrange
        var options = Options.Create(new RateLimitingMiddlewareOptions { RequestsPerMinute = 2 });
        var middleware = CreateMiddleware(options, out var context, out var mockNext);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        for (int i = 0; i < 2; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // Simulate time passing
        await Task.Delay(61000); // Wait slightly more than 1 minute

        // Reset the response for the next request
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Request should succeed after reset
        mockNext.Verify(next => next(context), Times.Exactly(3)); // Middleware should allow all 3 requests
    }

    // Helper to create the middleware and configure HttpContext
    private RateLimitingMiddleware CreateMiddleware(
        IOptions<RateLimitingMiddlewareOptions> options,
        out HttpContext context,
        out Mock<RequestDelegate> mockNext)
    {
        mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Avoid null stream issues

        return new RateLimitingMiddleware(mockNext.Object, options);
    }
}