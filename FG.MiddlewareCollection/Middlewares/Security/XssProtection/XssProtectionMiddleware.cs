using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class XssProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public XssProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestBodyStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            if (ContainsXssPayload(requestBodyText))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("XSS attack detected.");
                return;
            }

            await _next(context);
        }

        private bool ContainsXssPayload(string input)
        {
            // Extended check for XSS payloads
            var xssPatterns = new List<string>
            {
                "<script>",
                "</script>",
                "javascript:",
                "onerror=",
                "onload=",
                "<iframe>",
                "</iframe>",
                "<img",
                "src=",
                "eval(",
                "expression(",
                "vbscript:",
                "onmouseover="
            };

            foreach (var pattern in xssPatterns)
            {
                if (input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
