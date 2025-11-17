namespace Device.FingerPrint.Gateway.Configuration
{
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public int Database { get; set; } = 0;
        public string KeyPrefix { get; set; } = "ocelot_rate_limit:";
        public bool AllowAdmin { get; set; } = true;
    }
}