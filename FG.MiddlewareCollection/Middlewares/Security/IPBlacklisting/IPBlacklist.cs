using System.Collections.Generic;

namespace FG.MiddlewareCollection.Middlewares.Security
{
    public class IPBlacklist
    {
        public IEnumerable<string> BlacklistedIPs { get; set; }
    }
}
