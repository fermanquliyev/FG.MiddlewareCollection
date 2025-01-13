using FG.MiddlewareCollection.Middlewares.Performance;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Text;

namespace FG.MiddlewareCollection.Tests.UnitTests;

[TestClass]
public class ResponseCompressionMiddlewareTests
{
    [TestMethod]
    public async Task ShouldCompressResponseUsingGzip_WhenAcceptEncodingIsGzip()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, "gzip", "Test response");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual("gzip", context.Response.Headers["Content-Encoding"]);

        var compressedResponse = GetResponseBody(context);
        var decompressedResponse = DecompressGzip(compressedResponse);

        Assert.AreEqual("Test response", decompressedResponse);
    }

    [TestMethod]
    public async Task ShouldCompressResponseUsingBrotli_WhenAcceptEncodingIsBrotli()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, "br", "Test response");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual("br", context.Response.Headers["Content-Encoding"]);

        var compressedResponse = GetResponseBody(context);
        var decompressedResponse = DecompressBrotli(compressedResponse);

        Assert.AreEqual("Test response", decompressedResponse);
    }

    [TestMethod]
    public async Task ShouldNotCompress_WhenAcceptEncodingIsUnsupported()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, "unsupported-encoding", "Test Response");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsFalse(context.Response.Headers.ContainsKey("Content-Encoding"));

        var responseBody = GetResponseBody(context);
        Assert.AreEqual("Test Response", Encoding.UTF8.GetString(responseBody));
    }

    [TestMethod]
    public async Task ShouldPassThroughResponse_WhenNoAcceptEncodingHeader()
    {
        // Arrange
        var middleware = CreateMiddleware(out var context, null, "Test response");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsFalse(context.Response.Headers.ContainsKey("Content-Encoding"));

        var responseBody = GetResponseBody(context);
        Assert.AreEqual("Test response", Encoding.UTF8.GetString(responseBody));
    }

    // Helper to create the middleware and set up HttpContext
    private ResponseCompressionMiddleware CreateMiddleware(out HttpContext context, string acceptEncoding, string testResponse)
    {
        RequestDelegate mockNext = async (ctx) =>
        {
            await ctx.Response.WriteAsync(testResponse);
            ctx.Response.StatusCode = 200;
        };

        context = new DefaultHttpContext();

        if (acceptEncoding != null)
        {
            context.Request.Headers["Accept-Encoding"] = acceptEncoding;
        }

        context.Response.Body = new MemoryStream();

        return new ResponseCompressionMiddleware(mockNext);
    }

    // Helper to get the response body as a string
    private byte[] GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var memoryStream = new MemoryStream();
        context.Response.Body.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    // Helper to decompress Gzip
    private string DecompressGzip(byte[] compressedData)
    {
        using var memoryStream = new MemoryStream(compressedData);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    // Helper to decompress Brotli
    private string DecompressBrotli(byte[] compressedData)
    {
        using var memoryStream = new MemoryStream(compressedData);
        using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
        using var reader = new StreamReader(brotliStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
