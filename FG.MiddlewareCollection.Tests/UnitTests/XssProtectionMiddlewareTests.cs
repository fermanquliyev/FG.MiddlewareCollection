using FG.MiddlewareCollection.Middlewares.Security;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class XssProtectionMiddlewareUnitTests
{
    [TestMethod]
    public async Task ShouldAllowRequest_WhenNoXssPayloadIsDetected()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, out var mockNext);

        var validPayload = "Hello, this is a safe input.";
        SetRequestBody(context, validPayload);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Request should be allowed
        mockNext.Verify(next => next(context), Times.Once); // Middleware should call the next delegate
    }

    [TestMethod]
    public async Task ShouldBlockRequest_WhenXssPayloadIsDetected()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, out _);

        var maliciousPayload = "<script>alert('XSS');</script>";
        SetRequestBody(context, maliciousPayload);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(400, context.Response.StatusCode); // Bad Request
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseMessage = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.AreEqual("XSS attack detected.", responseMessage); // XSS warning message
    }

    [TestMethod]
    public async Task ShouldBlockRequest_WhenPayloadContainsComplexXssPatterns()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, out _);

        var complexPayload = "<img src='javascript:alert(1)'>";
        SetRequestBody(context, complexPayload);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(400, context.Response.StatusCode); // Bad Request
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseMessage = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.AreEqual("XSS attack detected.", responseMessage);
    }

    [TestMethod]
    public async Task ShouldHandleEmptyRequestBodyGracefully()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, out var mockNext);

        SetRequestBody(context, string.Empty);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(200, context.Response.StatusCode); // Request should be allowed
        mockNext.Verify(next => next(context), Times.Once); // Middleware should call the next delegate
    }

    // Helper to create the middleware and configure HttpContext
    private XssProtectionMiddleware CreateMiddleware(out HttpContext context, out Mock<RequestDelegate> mockNext)
    {
        mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // Set up response body for validation

        return new XssProtectionMiddleware(mockNext.Object);
    }

    // Helper to set the request body for the HttpContext
    private void SetRequestBody(HttpContext context, string bodyContent)
    {
        var requestBody = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent));
        context.Request.Body = requestBody;
        context.Request.ContentLength = requestBody.Length;
        context.Request.Body.Seek(0, SeekOrigin.Begin);
    }
}
