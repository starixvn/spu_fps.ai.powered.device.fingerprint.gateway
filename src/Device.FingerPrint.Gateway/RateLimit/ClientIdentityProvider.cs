using Ocelot.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace Device.FingerPrint.Gateway.RateLimit
{
    public class ClientIdentityProvider : IClientWhitelistService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientIdentityProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool WhitelistExists()
        {
            return false; // We'll handle this in custom logic
        }
    }

    public class IPAddressClientIdentifier : IClientIdService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPAddressClientIdentifier(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetClientId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "unknown";

            // Try to get the real IP address from X-Forwarded-For header first
            var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // Take the first IP in case of multiple IPs
                var firstIp = xForwardedFor.Split(',')[0].Trim();
                return firstIp;
            }

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}