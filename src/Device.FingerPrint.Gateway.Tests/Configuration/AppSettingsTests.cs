using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Device.FingerPrint.Gateway.Tests.Configuration
{
    public class AppSettingsTests
    {
        private IConfiguration CreateBasicConfiguration()
        {
            var configuration = Substitute.For<IConfiguration>();
            configuration["Environment"].Returns("TEST");
            configuration["IsUM"].Returns("false");
            configuration["GetSubscriberUrl"].Returns("http://test.com");
            configuration["Sentry:Dsn"].Returns("http://test-dsn.com");
            configuration["EnableMBDebugLogging"].Returns("false");
            return configuration;
        }

        [Fact]
        public void AppSettings_ShouldInitialize_WithValidConfiguration()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();

            // Act
            var appSettings = new AppSettings(configuration);

            // Assert
            Assert.NotNull(appSettings);
        }

        [Fact]
        public void Environment_ShouldReturnValue_WhenConfigured()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["Environment"].Returns("PRODUCTION");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var environment = appSettings.Environment;

            // Assert
            Assert.Equal("PRODUCTION", environment);
        }

        [Fact]
        public void IsUM_ShouldReturnTrue_WhenConfiguredAsTrue()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["IsUM"].Returns("true");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var isUM = appSettings.IsUM;

            // Assert
            Assert.True(isUM);
        }

        [Fact]
        public void IsUM_ShouldReturnFalse_WhenConfiguredAsFalse()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["IsUM"].Returns("false");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var isUM = appSettings.IsUM;

            // Assert
            Assert.False(isUM);
        }

        [Fact]
        public void SentryDsn_ShouldReturnValue_WhenConfigured()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["Sentry:Dsn"].Returns("http://test-sentry-dsn.com");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var sentryDsn = appSettings.SentryDsn;

            // Assert
            Assert.Equal("http://test-sentry-dsn.com", sentryDsn);
        }

        [Fact]
        public void GetSubscriberInfo_ShouldReturnValue_WhenConfigured()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["GetSubscriberUrl"].Returns("http://subscriber-service.com");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var subscriberUrl = appSettings.GetSubscriberInfo;

            // Assert
            Assert.Equal("http://subscriber-service.com", subscriberUrl);
        }

        [Fact]
        public void GetValue_ShouldReturnConfigurationValue_WhenKeyExists()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["TestKey"].Returns("TestValue");
            
            var appSettings = new AppSettings(configuration);

            // Act
            var value = appSettings.GetValue<string>("TestKey");

            // Assert
            Assert.Equal("TestValue", value);
        }

        [Fact]
        public void GetValue_ShouldReturnEmptyString_WhenKeyNotExists()
        {
            // Arrange
            var configuration = CreateBasicConfiguration();
            configuration["NonExistentKey"].Returns((string)null);
            
            var appSettings = new AppSettings(configuration);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => appSettings.GetValue<string>("NonExistentKey"));
        }
    }
}