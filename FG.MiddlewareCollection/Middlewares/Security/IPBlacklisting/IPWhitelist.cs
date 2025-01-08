using System.Collections.Generic;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class IPWhitelist
    {
        public IEnumerable<string> WhitelistedIPs { get; set; }
    }
}
