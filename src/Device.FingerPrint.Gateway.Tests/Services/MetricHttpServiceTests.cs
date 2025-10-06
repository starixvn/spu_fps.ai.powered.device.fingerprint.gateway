using Device.FingerPrint.Gateway.Services;
using Xunit;
using System.Reflection;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class MetricHttpServiceTests
    {
        private void ResetSingleton()
        {
            // Reset the static _instance field using reflection for isolation between tests
            var type = typeof(MetricHttpService);
            var field = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, null);
        }

        [Fact]
        public void PublicDataToGateway_ShouldInitializeSingletonInstance()
        {
            // Arrange
            ResetSingleton();
            var gatewayInfo = new GatewayInfo
            {
                Url = "http://localhost:9091",
                Username = "user",
                Password = "pass",
                Job = "testjob"
            };

            // Act
            MetricHttpService.PublicDataToGateway(gatewayInfo);

            // Assert
            var type = typeof(MetricHttpService);
            var field = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            var instance = field.GetValue(null);
            Assert.NotNull(instance);
            Assert.IsType<MetricHttpService>(instance);
        }

        [Fact]
        public void PublicDataToGateway_ShouldNotReinitializeSingletonInstance()
        {
            // Arrange
            ResetSingleton();
            var gatewayInfo1 = new GatewayInfo
            {
                Url = "http://localhost:9091",
                Username = "user1",
                Password = "pass1",
                Job = "job1"
            };
            var gatewayInfo2 = new GatewayInfo
            {
                Url = "http://localhost:9092",
                Username = "user2",
                Password = "pass2",
                Job = "job2"
            };

            // Act
            MetricHttpService.PublicDataToGateway(gatewayInfo1);
            var type = typeof(MetricHttpService);
            var field = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            var firstInstance = field.GetValue(null);

            MetricHttpService.PublicDataToGateway(gatewayInfo2);
            var secondInstance = field.GetValue(null);

            // Assert
            Assert.Same(firstInstance, secondInstance);
        }

        [Fact]
        public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
        {
            // Arrange
            ResetSingleton();
            var gatewayInfo = new GatewayInfo
            {
                Url = "http://localhost:9091",
                Username = "user",
                Password = "pass",
                Job = "testjob"
            };
            MetricHttpService.PublicDataToGateway(gatewayInfo);

            var type = typeof(MetricHttpService);
            var field = type.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            var instance = field.GetValue(null) as MetricHttpService;

            // Act & Assert
            instance.Dispose();
            instance.Dispose(); // Should not throw
        }
    }
}