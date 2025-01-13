using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class IPBlacklistingMiddlewareTests
{
    [TestMethod]
    public async Task ShouldReturnForbidden_WhenRequestIsFromBlacklistedIP()
    {
        // Arrange
        var blacklistedIPs = new IPBlacklist { BlacklistedIPs = new[] { "192.168.1.1" } };
        var middleware = CreateMiddleware(blacklistedIPs, out var context);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        Assert.IsFalse(context.Response.Body.Length > 0); // No body content expected
    }

    [TestMethod]
    public async Task ShouldCallNextMiddleware_WhenRequestIsFromNonBlacklistedIP()
    {
        // Arrange
        var blacklistedIPs = new IPBlacklist { BlacklistedIPs = new[] { "192.168.1.1" } };
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new IPBlacklistingMiddleware(mockNext.Object, blacklistedIPs);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.2");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Default status for a successful request
        mockNext.Verify(next => next(context), Times.Once); // Ensure next middleware was called
    }

    [TestMethod]
    public async Task ShouldHandleEmptyBlacklist_AndAllowAllIPs()
    {
        // Arrange
        var blacklistedIPs = new IPBlacklist { BlacklistedIPs = new string[0] }; // Empty blacklist
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new IPBlacklistingMiddleware(mockNext.Object, blacklistedIPs);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode);
        mockNext.Verify(next => next(context), Times.Once); // Ensure next middleware was called
    }

    // Helper to create the middleware and configure HttpContext
    private IPBlacklistingMiddleware CreateMiddleware(IPBlacklist blacklist, out HttpContext context)
    {
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Avoid null stream issues

        return new IPBlacklistingMiddleware(mockNext.Object, blacklist);
    }
}
