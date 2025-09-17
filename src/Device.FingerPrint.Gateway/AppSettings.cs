namespace Device.FingerPrint.Gateway
{
    using System.ComponentModel;

    public interface IAppSettings
    {
        bool IsUM { get; }

        string SentryDsn { get; }

        string GetSubscriberInfo { get; }

        string LogstashUrl { get; }

        string Environment { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly bool isUM;

        private readonly string sentryDsn;

        private readonly string getSubscriberInfo;

        private readonly string logstashUrl;

        private readonly string environment;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            isUM = GetValue<bool>("IsUM");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            getSubscriberInfo = GetValue<string>("GetSubscriberUrl");

            logstashUrl = GetValue<string>("Logstash:Url");

            environment = GetValue<string>("Environment");
        }

        public bool IsUM => isUM;

        public string SentryDsn => sentryDsn;

        public string GetSubscriberInfo => getSubscriberInfo;

        public string LogstashUrl => logstashUrl;

        public string Environment => environment;

        public T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                var typeConverter = TypeDescriptor.GetConverter(typeof(T));

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}
