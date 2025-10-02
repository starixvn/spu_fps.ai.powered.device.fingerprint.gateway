using Device.FingerPrint.Gateway.Models;
using Device.FingerPrint.Gateway.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Device.FingerPrint.Gateway.Controllers
{
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IAppSettings appSettings;
        private readonly RequestCounterService requestCounterService;
        public CommonController(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            this.requestCounterService = RequestCounterService.GetInstance();
        }

        [HttpGet("whoami")]
        public WhoamiModel Index()
        {
            var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            var localIpAddress = httpConnectionFeature?.LocalIpAddress;

            return new WhoamiModel
            {
                Environment = appSettings.Environment,
                ServerName = Environment.MachineName,
                ServerTime = DateTimeOffset.Now.DateTime,
                LocalIP = localIpAddress?.ToString() ?? string.Empty,
                ClientIP = GetHeaderIP("X-Remote-IP"),
                FakeIP = GetFakeIp(),
                RealIP = GetRealIp(),
                XFW = GetXForWarded(),
                ActualIP = GetHeaderIP("X-Actual-IP"),
                Counter = requestCounterService.RequestCount.ToString()
            };
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(',');

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }

        private string GetHeaderIP(string param)
        {
            var headerVal = HttpContext.Request.Headers[param].ToString();
            if (!string.IsNullOrEmpty(headerVal))
            {
                return headerVal;
            }

            return string.Empty;
        }

        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = HttpContext.Request.Host.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(',');

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return HttpContext.Request.Host.ToString();
                }
            }
            else
            {
                return proxyIP.Split(',')[0];
            }
        }
    }
}
