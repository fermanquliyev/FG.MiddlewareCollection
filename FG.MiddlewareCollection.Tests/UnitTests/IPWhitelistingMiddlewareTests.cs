using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class IPWhitelistingMiddlewareUnitTests
{
    [TestMethod]
    public async Task ShouldCallNextMiddleware_WhenRequestIsFromWhitelistedIP()
    {
        // Arrange
        var whitelistedIPs = new IPWhitelist { WhitelistedIPs = new[] { "192.168.1.1" } };
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new IPWhitelistingMiddleware(mockNext.Object, whitelistedIPs);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Default successful status
        mockNext.Verify(next => next(context), Times.Once); // Ensure next middleware is called
    }

    [TestMethod]
    public async Task ShouldReturnForbidden_WhenRequestIsFromNonWhitelistedIP()
    {
        // Arrange
        var whitelistedIPs = new IPWhitelist { WhitelistedIPs = new[] { "192.168.1.1" } };
        var middleware = CreateMiddleware(whitelistedIPs, out var context);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.2");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        Assert.IsFalse(context.Response.Body.Length > 0); // No body content expected
    }

    [TestMethod]
    public async Task ShouldBlockAllRequests_WhenWhitelistIsEmpty()
    {
        // Arrange
        var whitelistedIPs = new IPWhitelist { WhitelistedIPs = new string[0] }; // Empty whitelist
        var middleware = CreateMiddleware(whitelistedIPs, out var context);

        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    // Helper to create the middleware and configure HttpContext
    private IPWhitelistingMiddleware CreateMiddleware(IPWhitelist whitelist, out HttpContext context)
    {
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Avoid null stream issues

        return new IPWhitelistingMiddleware(mockNext.Object, whitelist);
    }
}
