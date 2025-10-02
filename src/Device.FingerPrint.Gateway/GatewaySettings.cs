namespace Device.FingerPrint.Gateway
{
    public class GatewayInfo
    {
        public string Url { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Job { get; set; } = string.Empty;

    }
    public interface IGatewaySettings
    {
        GatewayInfo GatewayInfo { get; }

        T GetSection<T>(string key);
    }
    public class GatewaySettings : IGatewaySettings
    {
        private readonly IConfiguration configuration;

        private readonly GatewayInfo gatewayInfo;

        public GatewaySettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            gatewayInfo = GetSection<GatewayInfo>("GatewayConfig") ?? new GatewayInfo();
        }

        public GatewayInfo GatewayInfo => gatewayInfo;

        public T GetSection<T>(string key)
        {
            var schedulerSection = configuration.GetSection(key);

            return schedulerSection.Get<T>();
        }
    }
}
