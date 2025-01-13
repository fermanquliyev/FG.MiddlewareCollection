using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class IPWhitelistingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<IPAddress> _whitelistedIPs;

        public IPWhitelistingMiddleware(RequestDelegate next, IPWhitelist whitelistedIPs)
        {
            _next = next;
            _whitelistedIPs = new HashSet<IPAddress>(whitelistedIPs.WhitelistedIPs?.Select(IPAddress.Parse));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (_whitelistedIPs.Contains(remoteIp))
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
