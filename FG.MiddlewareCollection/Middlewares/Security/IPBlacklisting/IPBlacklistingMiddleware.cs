using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class IPBlacklistingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<IPAddress> _blacklistedIPs;

        public IPBlacklistingMiddleware(RequestDelegate next, IPBlacklist blacklistedIPs)
        {
            _next = next;
            _blacklistedIPs = new HashSet<IPAddress>(blacklistedIPs.BlacklistedIPs?.Select(IPAddress.Parse));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (_blacklistedIPs.Contains(remoteIp))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next(context);
        }
    }
}
