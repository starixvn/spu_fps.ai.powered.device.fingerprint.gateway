using Device.FingerPrint.Gateway.Models;
using Prometheus;
using System.Net.Http.Headers;
using System.Text;

namespace Device.FingerPrint.Gateway.Services
{
    public class MetricHttpService : IDisposable
    {
        private static MetricHttpService? _instance;
        private static readonly object _lock = new object();
        private readonly MetricPusher? _pusher;
        private readonly HttpClient? _httpClient;
        private bool _disposed;

        public MetricHttpService()
        {
            // Default constructor for DI
        }

        private MetricHttpService(GatewayInfo gatewayInfo)
        {
            var headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{gatewayInfo.Username}:{gatewayInfo.Password}"));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);

            _pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = gatewayInfo.Url,
                Job = gatewayInfo.Job,
                Instance = Environment.MachineName,
                HttpClientProvider = () => _httpClient
            });

            _pusher.Start();
        }

        public static void PublicDataToGateway(GatewayInfo gatewayInfo)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new MetricHttpService(gatewayInfo);
                }
            }
        }

        public static void DisposeInstance()
        {
            if (_instance != null)
            {
                lock (_lock)
                {
                    _instance?.Dispose();
                    _instance = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pusher?.Stop();
                    _pusher?.Dispose();
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MetricHttpService()
        {
            Dispose(false);
        }
    }
}
