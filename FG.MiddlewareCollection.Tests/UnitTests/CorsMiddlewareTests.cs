using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class CorsMiddlewareTests
{
    [TestMethod]
    public async Task ShouldAddCorsHeaders_WhenRequestIsMade()
    {
        // Arrange
        var options = CreateCorsOptions();
        var middleware = CreateMiddleware(options, out var context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(options.AllowOrigin, context.Response.Headers["Access-Control-Allow-Origin"]);
        Assert.AreEqual(options.AllowMethods, context.Response.Headers["Access-Control-Allow-Methods"]);
        Assert.AreEqual(options.AllowHeaders, context.Response.Headers["Access-Control-Allow-Headers"]);
    }

    [TestMethod]
    public async Task ShouldReturn204ForOptionsRequest_WhenRequestMethodIsOptions()
    {
        // Arrange
        var options = CreateCorsOptions();
        var middleware = CreateMiddleware(options, out var context);

        context.Request.Method = "OPTIONS";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(204, context.Response.StatusCode);
        Assert.AreEqual(options.AllowOrigin, context.Response.Headers["Access-Control-Allow-Origin"]);
        Assert.AreEqual(options.AllowMethods, context.Response.Headers["Access-Control-Allow-Methods"]);
        Assert.AreEqual(options.AllowHeaders, context.Response.Headers["Access-Control-Allow-Headers"]);
    }

    [TestMethod]
    public async Task ShouldPassToNextMiddleware_WhenRequestIsNotOptions()
    {
        // Arrange
        var options = CreateCorsOptions();
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new CorsMiddleware(mockNext.Object, Options.Create(options));

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Default status for successful path
        mockNext.Verify(next => next(context), Times.Once);
    }

    // Helper to create default CORS options
    private CorsMiddlewareOptions CreateCorsOptions()
    {
        return new CorsMiddlewareOptions
        {
            AllowOrigin = "*",
            AllowMethods = "GET, POST, OPTIONS",
            AllowHeaders = "Content-Type, Authorization"
        };
    }

    // Helper to create the middleware and set up HttpContext
    private CorsMiddleware CreateMiddleware(CorsMiddlewareOptions options, out HttpContext context)
    {
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Avoid null stream exception

        return new CorsMiddleware(mockNext.Object, Options.Create(options));
    }
}