using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Performance
{
    public class ResponseCompressionMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseCompressionMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();

            if (string.IsNullOrEmpty(acceptEncoding))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;

                try
                {
                    await _next(context);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var compressedStream = new MemoryStream())
                    {
                        Stream compressionStream = null;
                        if (acceptEncoding.Contains("br"))
                        {
                            context.Response.Headers["Content-Encoding"] = "br";
                            compressionStream = new BrotliStream(compressedStream, CompressionLevel.Fastest, true);
                        }
                        else if (acceptEncoding.Contains("gzip"))
                        {
                            context.Response.Headers["Content-Encoding"] = "gzip";
                            compressionStream = new GZipStream(compressedStream, CompressionLevel.Fastest, true);
                        }
                        else if (acceptEncoding.Contains("deflate"))
                        {
                            context.Response.Headers["Content-Encoding"] = "deflate";
                            compressionStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true);
                        }
                        else
                        {
                            // No compression, just copy the memoryStream to originalBodyStream
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            await memoryStream.CopyToAsync(originalBodyStream);
                            return;
                        }

                        if (compressionStream != null)
                        {
                            await memoryStream.CopyToAsync(compressionStream);
                            await compressionStream.FlushAsync();

                            compressedStream.Seek(0, SeekOrigin.Begin);
                            context.Response.Headers.Remove("Content-Length");
                            await compressedStream.CopyToAsync(originalBodyStream);
                        }
                    }
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }
    }
}
