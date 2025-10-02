namespace Device.FingerPrint.Gateway
{
    using System.ComponentModel;

    public interface IAppSettings
    {
        bool IsUM { get; }

        string SentryDsn { get; }

        string GetSubscriberInfo { get; }

        string Environment { get; }

        bool EnableMBDebugLogging { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly bool isUM;

        private readonly string sentryDsn;

        private readonly string getSubscriberInfo;

        private readonly string environment;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            isUM = GetValue<bool>("IsUM");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            getSubscriberInfo = GetValue<string>("GetSubscriberUrl");

            environment = GetValue<string>("Environment");

            EnableMBDebugLogging = GetValue<bool>("EnableMBDebugLogging");
        }

        public bool IsUM => isUM;

        public string SentryDsn => sentryDsn;

        public string GetSubscriberInfo => getSubscriberInfo;

        public string Environment => environment;

        public bool EnableMBDebugLogging { get;  }

        public T GetValue<T>(string key)
        {
            var value = configuration[key];

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for key '{key}' is null or empty.");
            }

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));

            if (typeConverter == null)
            {
                throw new InvalidOperationException($"No type converter available for type '{typeof(T)}'.");
            }

            var convertedValue = typeConverter.ConvertFromString(value);

            if (convertedValue == null)
            {
                throw new InvalidCastException($"Failed to convert configuration value for key '{key}' to type '{typeof(T)}'.");
            }

            return (T)convertedValue;
        }
    }
}
